using RestoreApiV2.Entities.OrderAggregate;

namespace RestoreApiV2.DTOs
{
    public class CreateOrderDto
    {
        public required ShippingAddress ShippingAddress { get; set; }
        public required PaymentSummary PaymentSummary { get; set; }
    }
}
