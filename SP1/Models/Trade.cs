namespace SP1.Models;

public class Trade : EntityBase
{
    [Attributes.ForeignEntityAttribute(typeof(Client))]
    public int? Client { get; set; } = null;
    [Attributes.ForeignEntityAttribute(typeof(Manager))]
    public int? Manager { get; set; } = null;
    public TradeStatus Status { get; set; } = TradeStatus.New;
}

public enum TradeStatus
{
    New,
    Process,
    Complete,
    Break,
    Failed
}
