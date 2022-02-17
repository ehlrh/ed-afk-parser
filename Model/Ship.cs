namespace AfkParse.Model;

public class Ship
{
    public Ship(string name, string affiliation)
    {
        Name = name;
        Affiliation = affiliation;
    }

    public string Name { get; private set; }
    public string Affiliation { get; private set; }
}