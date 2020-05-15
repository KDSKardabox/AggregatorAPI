using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AggregatorAPI.Controllers
{
  [ApiController]
  [Route("[controller]")]

  public class OrderDetailsController : ControllerBase
  {
    public OrderDetailsController(IConfiguration configuration, IHttpClientFactory clientFactory)
    {
      Configuration = configuration;
      _clientFactory = clientFactory;
    }

    private IHttpClientFactory _clientFactory;

    public IConfiguration Configuration { get; }

    public string UsersAPIUrlPrefix
    {
      get
      {
        return String.Format("http://{0}:{1}/{2}/",
          Configuration["UsersAPI:Host"],
          Configuration["UsersAPI:Port"],
          Configuration["UsersAPI:Uri"]
        );

      }
    }

    public string OrdersAPIUrlPrefix
    {
      get
      {
        return String.Format("http://{0}:{1}/{2}/",
          Configuration["OrdersAPI:Host"],
          Configuration["OrdersAPI:Port"],
          Configuration["OrdersAPI:Uri"]
        );

      }
    }

    [HttpGet("{id}")]
    public async Task<object> Get(string id)
    {
      string userUrl = String.Format(UsersAPIUrlPrefix) + id;
      string orderUrl = String.Format(OrdersAPIUrlPrefix) + id;
      string errorMessage = null;
      OrderDetail orderDetails = new OrderDetail() { };
      try
      {
        using (var client = _clientFactory.CreateClient())
        {
          using (var userResponse = await client.GetAsync(userUrl))
          {
            if (userResponse.IsSuccessStatusCode)
            {
              var userJson = await userResponse.Content.ReadAsStringAsync();
              orderDetails.UserDetails = JsonConvert.DeserializeObject<User>(userJson);

              using (var orderResponse = await client.GetAsync(orderUrl))
              {
                if (orderResponse.IsSuccessStatusCode)
                {
                  var orderJson = await orderResponse.Content.ReadAsStringAsync();
                  orderDetails.Orders = JsonConvert.DeserializeObject<IList<Order>>(orderJson);
                }
                else
                {
                  errorMessage = orderResponse.StatusCode.ToString();
                  return errorMessage;
                }
              }
            }
            else
            {
              errorMessage = userResponse.StatusCode.ToString();
              return errorMessage;
            }
          }
        }
      }
      catch (Exception e)
      {
        errorMessage = e.Message;
        return errorMessage;
      }
      return orderDetails;
    }
  }
}
