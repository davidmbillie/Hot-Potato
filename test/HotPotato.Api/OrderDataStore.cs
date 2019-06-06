﻿using HotPotato.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotPotato.Api
{
    public class OrderDataStore
    {
        public static OrderDataStore Current { get; set; } = new OrderDataStore();
        public List<Order> Orders { get; set; }

        public OrderDataStore()
        {
            Orders = new List<Order>()
            {
                new Order()
                {
                    Id = 1,
                    Price = 30.00,
                    Items = new List<Item>()
                    {
                        new Item()
                        {
                            ItemId = 1,
                            Name = "Paper",
                            Price = 10.00
                        },
                        new Item()
                        {
                            ItemId = 2,
                            Name = "Pencils",
                            Price = 10.00
                        },new Item()
                        {
                            ItemId = 3,
                            Name = "Pens",
                            Price = 10.00
                        }
                    }
                },
                new Order()
                {
                    Id = 2,
                    Price = 15.00,
                    Items = new List<Item>()
                    {
                        new Item()
                        {
                            ItemId = 4,
                            Name = "Post-Its",
                            Price = 5.00
                        },
                        new Item()
                        {
                            ItemId = 5,
                            Name = "Markers",
                            Price = 10.00
                        }
                    }
                }
            };
        }
    }
}
