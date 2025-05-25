using CargohubV2.Models;
using CargohubV2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Controllers
{
    [ApiController]
    [Route("api/v1/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetAll([FromQuery] int? limit)
        {
            var entities = limit.HasValue
                ? await _orderService.GetOrdersAsync(limit.Value)
                : await _orderService.GetAllOrdersAsync();
            return Ok(entities);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Order>> GetById(int id)
        {
            var entity = await _orderService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        [HttpPost]
        public async Task<ActionResult<Order>> Create([FromBody] Order order)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _orderService.AddOrderAsync(order);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Order>> Update(int id, [FromBody] Order updated)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _orderService.UpdateOrderAsync(id, updated);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _orderService.SoftDeleteByIdAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
