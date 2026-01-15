using HotPotato.Test.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;

namespace HotPotato.Test.Api.Controllers
{
	public class OrdersController : Controller
	{
		#region HAPPY PATHS

		[HttpGet("/")]
		public IActionResult getLandingPage()
		{
			string repo = @"https://github.com/HylandSoftware/Hot-Potato";

			return Ok(repo);
		}

		[HttpGet("/order")]
		public IActionResult GetOrders()
		{
			return Ok(OrderDataStore.Current.Orders);
		}

		[HttpPost("/order")]
		public IActionResult PostOrder([FromBody] Order order)
		{
			ProblemDetails problemDetails = new ProblemDetails()
			{
				Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
				Status = 400,
				Title = "400 Bad Request",
				Detail = "Order needs an ID, Price, and Items[]",
				Instance = GetInstance(HttpContext.Request)
			};

			if (OrderDataStore.Current.Orders.FirstOrDefault(r => r.Id == order.Id) != null)
			{
				problemDetails.Detail = $"Order with ID: {order.Id} already exists";
				return new ObjectResult(problemDetails)
				{
					StatusCode = problemDetails.Status,
					ContentTypes = new Microsoft.AspNetCore.Mvc.Formatters.MediaTypeCollection() { new MediaTypeHeaderValue("application/problem+json") }
				};
			}

			if (order.Id == 0 || order.Price == 0 || order.Items == null)
			{
                

				return new ObjectResult(problemDetails)
				{
					StatusCode = problemDetails.Status,
					ContentTypes = new Microsoft.AspNetCore.Mvc.Formatters.MediaTypeCollection() { new MediaTypeHeaderValue("application/problem+json") }
				};
			}

			var uri = $"https://localhost:44366/order/{order.Id}";

			return Created(uri, order);
		}

		[HttpGet("/order/{id}")]
		public IActionResult GetOrderWithId(int id)
		{
			//Invalid Body condition -- Missing properties
			if (id == 555)
			{
				var t = new { Price = 20.50 };

				return Ok(t);
			}

			//Missing Body -- return Empty Body
			if(id == 777)
			{
				return Ok();
			}

			//MissingContent -- return content-type not listed
			if(id == 888)
			{
				return Ok("Wrong Content-Type");
			}

			var order = OrderDataStore.Current.Orders.FirstOrDefault(r => r.Id == id);

			if(order == null)
			{
				ProblemDetails problemDetails = new ProblemDetails()
				{
					Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
					Status = 404,
					Title = "404 Not Found",
					Detail = $"Order with ID: {id} was not found",
					Instance = GetInstance(HttpContext.Request)
				};

				return new ObjectResult(problemDetails)
				{
					StatusCode = problemDetails.Status,
					ContentTypes = new Microsoft.AspNetCore.Mvc.Formatters.MediaTypeCollection() { new MediaTypeHeaderValue("application/problem+json") }
				};
			}

			return Ok(order);
		}

		[HttpPut("/order/{id}")]
		public IActionResult PutOrderWithId([FromBody] Order oldOrder, int id)
		{
			var order = OrderDataStore.Current.Orders.FirstOrDefault(r => r.Id == id);

			if (order == null)
			{
				ProblemDetails problemDetails = new ProblemDetails()
				{
					Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
					Status = 404,
					Title = "404 Not Found",
					Detail = $"Order with ID: {id} was not found",
					Instance = GetInstance(HttpContext.Request)
				};

				return new ObjectResult(problemDetails)
				{
					StatusCode = problemDetails.Status,
					ContentTypes = new Microsoft.AspNetCore.Mvc.Formatters.MediaTypeCollection() { new MediaTypeHeaderValue("application/problem+json") }
				};
			}

			OrderDataStore.Current.Orders[OrderDataStore.Current.Orders.FindIndex(r => r.Id == id)] = oldOrder;

			return NoContent();
		}

		[HttpOptions("/order/{id}")]
		public IActionResult OptionsForPath()
		{
			Response.Headers.Append("Allow", "Allow");
			return Ok("GET, PUT");
		}

		[HttpGet("/order/{id}/price")]
		public IActionResult GetPriceWithId(int id)
		{

			var order = OrderDataStore.Current.Orders.FirstOrDefault(r => r.Id == id);

			if (order == null)
			{
				ProblemDetails problemDetails = new ProblemDetails()
				{
					Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
					Status = 404,
					Title = "404 Not Found",
					Detail = $"Order with ID: {id} was not found",
					Instance = GetInstance(HttpContext.Request)
				};

				return new ObjectResult(problemDetails)
				{
					StatusCode = problemDetails.Status,
					ContentTypes = new Microsoft.AspNetCore.Mvc.Formatters.MediaTypeCollection() { new MediaTypeHeaderValue("application/problem+json") }
				};
			}

			var price = String.Format("{0:.00}", order.Price);

			return Ok(price);
		}

		[HttpGet("/order/{id}/items")]
		public IActionResult GetItemsForOrderId(int id)
		{
			var order = OrderDataStore.Current.Orders.FirstOrDefault(r => r.Id == id);

			if (order == null)
			{
				ProblemDetails problemDetails = new ProblemDetails()
				{
					Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
					Status = 404,
					Title = "404 Not Found",
					Detail = $"Order with ID: {id} was not found",
					Instance = GetInstance(HttpContext.Request)
				};

				return new ObjectResult(problemDetails)
				{
					StatusCode = problemDetails.Status,
					ContentTypes = new Microsoft.AspNetCore.Mvc.Formatters.MediaTypeCollection() { new MediaTypeHeaderValue("application/problem+json") }
				};
			}

			return Ok(order.Items);
		}

		[HttpGet("/order/{id}/items/{itemId}")]
		public IActionResult GetItemWithIdFromOrderWithId(int id, int itemId)
		{
			var order = OrderDataStore.Current.Orders.FirstOrDefault(r => r.Id == id);
			var item = OrderDataStore.Current.Orders.FirstOrDefault(r => r.Id == id).Items.FirstOrDefault(r => r.ItemId == itemId);

			if (order == null || item == null)
			{
				ProblemDetails problemDetails = new ProblemDetails()
				{
					Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
					Status = 404,
					Title = "404 Not Found",
					Detail = $"Order with ID: {id} was not found or Item with ID: {itemId} was not found",
					Instance = GetInstance(HttpContext.Request)
				};

				return new ObjectResult(problemDetails)
				{
					StatusCode = problemDetails.Status,
					ContentTypes = new Microsoft.AspNetCore.Mvc.Formatters.MediaTypeCollection() { new MediaTypeHeaderValue("application/problem+json") }
				};
			}

			var res = new ContentResult()
			{
				ContentType = "application/pdf",
				StatusCode = 200,
				Content = item.ToString()
			};

			return res;
		}

		[HttpDelete("/order/{id}/items/{itemId}")]
		public IActionResult DeleteItemWithItemIdInOrder(int id, int itemId)
		{

			//InValidHeaders -- strings not formatted correctly
			if (id == 666 && itemId == 666)
			{
				Response.Headers.Append("X-header", "&*(&^&%%##@");
				return NoContent();
			}

			//MissingHeaders -- Omit Header defined in spec
			if(id == 777 && itemId == 777)
			{
				Response.Headers.Clear();
				return NoContent();    
			}

			var order = OrderDataStore.Current.Orders.FirstOrDefault(r => r.Id == id);
			var item = OrderDataStore.Current.Orders.FirstOrDefault(r => r.Id == id).Items.FirstOrDefault(r => r.ItemId == itemId);

			if (order == null || item == null)
			{
				ProblemDetails problemDetails = new ProblemDetails()
				{
					Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
					Status = 404,
					Title = "404 Not Found",
					Detail = $"Order with ID: {id} was not found or Item with ID: {itemId} was not found",
					Instance = GetInstance(HttpContext.Request)
				};

				return new ObjectResult(problemDetails)
				{
					StatusCode = problemDetails.Status,
					ContentTypes = new Microsoft.AspNetCore.Mvc.Formatters.MediaTypeCollection() { new MediaTypeHeaderValue("application/problem+json") }
				};
			}

			var res = new NoContentResult();

			Response.Headers.Append("X-header", "SEVBREVS");

			return res;
		}

		#endregion

		#region NOT IN SPEC PATHS

		/// <summary>
		/// PATH NOT IN SPEC! HotPotato should catch this call and return an error.
		/// </summary>
		/// <returns></returns>
		[HttpGet("/missingpath")]
		public IActionResult GetMissingPath()
		{
			var path = "This PATH is missing in the spec!";
			return Ok(path);
		}

		/// <summary>
		/// METHOD NOT IN SPEC! HotPotato should catch this call and return an error.
		/// </summary>
		/// <returns></returns>
		[HttpGet("/missingmethod")]
		public IActionResult GetMissingMethod()
		{
			var method = "This METHOD is missing in the spec!";
			return Ok(method);
		}

		/// <summary>
		/// STATUS CODE NOT IN SPEC! HotPotato should catch this call and return an error.
		/// </summary>
		/// <returns></returns>
		[HttpGet("/missingcode")]
		public IActionResult GetMissingStatusCode()
		{
			var code = "This STATUS CODE is missing in the spec!";
			return Ok(code);
		}

		#endregion

		private static string GetInstance(HttpRequest request)
		{
			return $"{request.Scheme}://{request.Host.ToUriComponent()}{request.Path.Value}";
		}
	}
}
