using System.Collections.Generic;
public class OrderDetail
{
  public User UserDetails { get; set; }
  public IList<Order> Orders { get; set; }
}
