﻿using Dapper;
using EdaSample.Common.Events;
using EdaSample.Services.Customer.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace EdaSample.Services.Customer.Controllers
{
    [Route("api/[controller]")]
    public class CustomersController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly string connectionString;
        private readonly IEventBus eventBus;
        private readonly ILogger logger;

        public CustomersController(IConfiguration configuration,
            IEventBus eventBus,
            ILogger<CustomersController> logger)
        {
            this.configuration = configuration;
            this.connectionString = configuration["mssql:connectionString"];
            this.eventBus = eventBus;
            this.logger = logger;
        }


        // 获取指定ID的客户信息
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            const string sql = "SELECT [CustomerId] AS Id, [CustomerName] AS Name FROM [dbo].[Customers] WHERE [CustomerId]=@id";
            using (var connection = new SqlConnection(connectionString))
            {
                var customer = await connection.QueryFirstOrDefaultAsync<Model.Customer>(sql, new { id });
                if (customer == null)
                {
                    return NotFound();
                }

                return Ok(customer);
            }
        }

        // 创建新的客户信息
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] dynamic model)
        {
            this.logger.LogInformation($"开始创建客户信息。");
            var name = (string)model.Name;
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest();
            }

            const string sql = "INSERT INTO [dbo].[Customers] ([CustomerId], [CustomerName]) VALUES (@Id, @Name)";
            using (var connection = new SqlConnection(connectionString))
            {
                var customer = new Model.Customer(name);
                await connection.ExecuteAsync(sql, customer);

                await this.eventBus.PublishAsync(new CustomerCreatedEvent(name));

                this.logger.LogInformation($"客户信息创建成功。");

                return Created(Url.Action("Get", new { id = customer.Id }), customer.Id);
            }
        }
    }
}
