﻿using Microsoft.EntityFrameworkCore;

namespace RestoreApiV2.Entities.OrderAggregate
{
    [Owned]
    public class ProductItemOrdered
    {
        public int ProductId { get; set; }
        public required string Name { get; set; }
        public required string PictureUrl { get; set; }
    }
}
