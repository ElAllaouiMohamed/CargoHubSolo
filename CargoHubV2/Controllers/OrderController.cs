using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly ILoggingService _loggingService;

        public OrdersController(OrderService orderService, ILoggingService loggingService)
        {
            _orderService = orderService;
            _loggingService = loggingService;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all orders", Description = "Returns a list of orders with optional limit.")]
        [SwaggerResponse(200, "List of orders", typeof(IEnumerable<Order>))]
        public async Task<ActionResult<IEnumerable<Order>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _orderService.GetOrdersAsync(limit.Value)
                : await _orderService.GetAllOrdersAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get order by ID", Description = "Returns a single order by its unique identifier.")]
        [SwaggerResponse(200, "Order found", typeof(Order))]
        public async Task<ActionResult<Order>> GetById(int id)
        {
            var entity = await _orderService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new order", Description = "Adds a new order to the system.")]
        [SwaggerResponse(201, "Order created", typeof(Order))]
        public async Task<ActionResult<Order>> Create([FromBody] Order order)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _orderService.AddOrderAsync(order);

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Order",
                "Create",
                HttpContext.Request.Path,
                $"Order created with Id {created.Id}");

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [SwaggerOperation(Summary = "Update an existing order", Description = "Updates an existing order in the system.")]
        [SwaggerResponse(200, "Order updated", typeof(Order))]
        public async Task<ActionResult<Order>> Update(int id, [FromBody] Order updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _orderService.UpdateOrderAsync(id, updated);
            if (result == null)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Order",
                "Update",
                HttpContext.Request.Path,
                $"Order {id} updated");

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [SwaggerOperation(Summary = "Delete an order", Description = "Deletes an existing order by its unique identifier.")]
        [SwaggerResponse(204, "Order deleted")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _orderService.SoftDeleteByIdAsync(id);
            if (!result)
                return NotFound();

            await _loggingService.LogAsync(
                User?.Identity?.Name ?? "anonymous",
                "Order",
                "Delete",
                HttpContext.Request.Path,
                $"Order {id} soft-deleted");

            return NoContent();
        }
    }
}

