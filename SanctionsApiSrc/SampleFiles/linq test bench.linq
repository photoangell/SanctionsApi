<Query Kind="Program" />

void Main() {
var row = new String[4];
row[0] = "bob billy";
row[1] = "bobr billy";
row[2] = "brob billy";
row[3] = "Vlad billy";
	
var name = new List<string>();
name.Add("jeff");
name.Add("bob");
name.Add("vlad");

	var name2 = new List<string>();
	name2.Add("vlad");
	name2.Add("naumov");


	var fullnames = new List<FullName>();

	fullnames.Add(new FullName() {
		Name = name
	});
	fullnames.Add(new FullName() {
		Name = name2
	});


	row.Dump("row to look at");

	row.SelectMany(r => r.Split(' '))
		.Distinct()
		.Dump("names to search, flattened");

	
	name.Dump("names we are looking for");

	//fullnames.SelectMany(f => f.Name)
	//	.Dump();

	row.SelectMany(r => r.Split(' '))
		.Distinct()
		.Join(name,
		r => r.ToLower(),
		n => n.ToLower(),
		(r, n) => new {r})
		.Count()
		.Dump();

	

}

// Define other methods and classes here
public class FullName {
	public List<string> Name { get; set; } = new List<string>();

	public override string ToString() {
		return String.Join(" ", Name);
	}
	public int MaxAllowedScore => Name.Count > 2 ? 2 : Name.Count;
}