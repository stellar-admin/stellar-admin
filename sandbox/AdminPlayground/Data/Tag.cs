namespace AdminPlayground.Data;

public sealed class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public List<Product> Products { get; } = [];
}
