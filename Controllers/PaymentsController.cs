using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestoreApiV2.Data;
using RestoreApiV2.DTOs;
using RestoreApiV2.Entities.OrderAggregate;
using RestoreApiV2.Extensions;
using RestoreApiV2.Services;
using Stripe;
using System.Runtime.InteropServices;

namespace RestoreApiV2.Controllers
{
    public class PaymentsController(PaymentsService paymentsService,
        StoreContext context, IConfiguration config, ILogger<PaymentsController> logger) : BaseApiController
    {
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<BasketDto>> CreateOrUpdatePaymentIntent()
        {
            var basket = await context.Baskets.GetBasketWithItems(Request.Cookies["basketId"]);

            if (basket == null) 
                return BadRequest("Problem with the basket");

            var intent = await paymentsService.CreateOrUpdatePaymentIntent(basket);

            if (intent == null) 
                return BadRequest("Problem creating payment intent");

            basket.PaymentIntentId ??= intent.Id;
            basket.ClientSecret ??= intent.ClientSecret;

            if (context.ChangeTracker.HasChanges())
            {
                var result = await context.SaveChangesAsync() > 0;

                if (!result) 
                    return BadRequest("Problem updating basket with intent");
            }

            return basket.ToDto();
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            logger.LogInformation("Stripe webhook received: {Json}", json);

            try
            {
                var stripeEvent = ConstructStripeEvent(json);
                logger.LogInformation("🔍 Stripe event type: {Type}", stripeEvent.Type);
                logger.LogInformation("🆔 Stripe event ID: {Id}", stripeEvent.Id);

                if (stripeEvent.Data.Object is not PaymentIntent intent)
                {
                    logger.LogWarning("⚠️ Received event with invalid data object.");
                    return BadRequest("Invalid event data");
                }

                logger.LogInformation("💳 PaymentIntent ID: {IntentId}, Status: {Status}", intent.Id, intent.Status);

                if (intent.Status == "succeeded") 
                    await HandlePaymentIntentSucceeded(intent);
                else 
                    await HandlePaymentIntentFailed(intent);

                return Ok();
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "Stripe webhook error");
                return StatusCode(StatusCodes.Status500InternalServerError, "Webhook error");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An expected error has occurred");
                return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error");
            }
        }

        private async Task HandlePaymentIntentFailed(PaymentIntent intent)
        {
            var order = await context.Orders
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync(x => x.PaymentIntentId == intent.Id)
                    ?? throw new Exception("Order not found");

            foreach (var item in order.OrderItems)
            {
                var productItem = await context.Products
                    .FindAsync(item.ItemOrdered.ProductId)
                        ?? throw new Exception("Problem updating order stock");

                productItem.QuantityInStock += item.Quantity;
            }

            order.OrderStatus = OrderStatus.PaymentFailed;

            await context.SaveChangesAsync();
        }

        private async Task HandlePaymentIntentSucceeded(PaymentIntent intent)
        {
            var order = await context.Orders
               .Include(x => x.OrderItems)
               .FirstOrDefaultAsync(x => x.PaymentIntentId == intent.Id)
                   ?? throw new Exception("Order not found");

            if (order.GetTotal() != intent.Amount)
            {
                order.OrderStatus = OrderStatus.PaymentMismatch;
            }
            else
            {
                order.OrderStatus = OrderStatus.PaymentReceived;
            }

            var basket = await context.Baskets.FirstOrDefaultAsync(x =>
                x.PaymentIntentId == intent.Id);

            if (basket != null) 
                context.Baskets.Remove(basket);

            await context.SaveChangesAsync();
        }

        private Event ConstructStripeEvent(string json)
        {
            try
            {
                var sec = config["StripeSettings:WhSecret"];
                logger.LogInformation("Secret = " + sec);
                return EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], config["StripeSettings:WhSecret"]);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to construct stripe event");
                throw new StripeException("Invalid signature");
            }
        }
    }
}
