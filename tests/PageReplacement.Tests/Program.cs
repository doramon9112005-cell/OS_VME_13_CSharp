using System.Runtime.InteropServices;
using System.Text.Json;
using PageReplacement.Core;

string root = Path.GetFullPath(args.Length > 0 ? args[0] : ".");
var tests = new List<(string Name, Action Test)>();
int[] textbook = [7,0,1,2,0,3,0,4,2,3,0,3,0,3,2,1,2,0,1,7,0,1];
int[] belady = [1,2,3,4,1,2,5,1,2,3,4,5];
void Test(string name, Action action) => tests.Add((name, action));
void Equal<T>(T expected, T actual) { if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"Expected {expected}, actual {actual}"); }
void Sequence<T>(IEnumerable<T> expected,IEnumerable<T> actual) { if (!expected.SequenceEqual(actual)) throw new Exception("Sequences differ"); }
void Throws(Action action) { try { action(); } catch (Exception ex) when (ex is ArgumentException or FormatException) { return; } throw new Exception("Expected validation exception"); }

Test("Slide oracle FIFO LRU OPT", () => { Equal(15,Simulator.Run(textbook,3,Policy.FIFO).Faults);Equal(12,Simulator.Run(textbook,3,Policy.LRU).Faults);Equal(9,Simulator.Run(textbook,3,Policy.OPT).Faults); });
Test("Repeated page", () => { foreach(var p in Policies.All) { var r=Simulator.Run([5,5,5,5],3,p);Equal((1,3,0),(r.Faults,r.Hits,r.Replacements)); } });
Test("Single frame", () => { foreach(var p in Policies.All) Equal(4,Simulator.Run([1,2,2,3,1],1,p).Faults); });
Test("All pages fit", () => { foreach(var p in Policies.All) Equal(3,Simulator.Run([0,1,2,0,1,2],8,p).Faults); });
Test("Page zero victim", () => { foreach(var p in Policies.All) { var r=Simulator.Run([0,1],1,p); Equal<int?>(0,r.Steps[1].Victim);Equal(1,r.Replacements); } });
Test("FIFO hit preserves admission order", () => { var r=Simulator.Run([1,2,1,3],2,Policy.FIFO);Equal<int?>(1,r.Steps[^1].Victim);Sequence(new[]{1,2},r.Steps[2].Order); });
Test("LRU hit updates recency", () => { var r=Simulator.Run([1,2,1,3],2,Policy.LRU);Equal<int?>(2,r.Steps[^1].Victim);Sequence(new[]{2,1},r.Steps[2].Order); });
Test("OPT infinity tie lowest slot", () => { var s=Simulator.Run([1,2,3,4],3,Policy.OPT).Steps[^1];Equal(0,s.Slot);Equal<int?>(1,s.Victim); });
Test("Clock exact bits hand and victims", () =>
{
    var r=Simulator.Run([1,2,3,2,4,2,5],3,Policy.SecondChance);Equal(5,r.Faults);
    Sequence(new int?[]{1,2,0,0,1,1,0},r.Steps.Select(s=>s.Hand));
    Sequence(new[]{0,1,2},r.Steps[4].ClearedSlots);Sequence(new[]{1,0,0},r.Steps[4].Bits);
    Sequence(new[]{1,1,0},r.Steps[5].Bits);Sequence(new int?[]{4,2,5},r.Steps[6].Frames);Equal<int?>(3,r.Steps[6].Victim);
});
Test("Belady 9 to 10", () => { Equal(9,Simulator.Run(belady,3,Policy.FIFO).Faults);Equal(10,Simulator.Run(belady,4,Policy.FIFO).Faults); });
Test("Trace versus summary mode", () => { foreach(var p in Policies.All) { var a=Simulator.Run(textbook,3,p);var b=Simulator.Run(textbook,3,p,false);Equal((a.Faults,a.Replacements),(b.Faults,b.Replacements));Equal(0,b.Steps.Count); } });
Test("Invalid model inputs and trace limit", () =>
{
    foreach(int f in new[]{0,-1,257}) Throws(()=>Simulator.Run([1],f,Policy.FIFO));
    Throws(()=>Simulator.Run([],3,Policy.FIFO));Throws(()=>Simulator.Run([-1],3,Policy.FIFO));
    Throws(()=>Simulator.Run([1],3,(Policy)99));Throws(()=>Simulator.Run(new int[200001],3,Policy.FIFO));
    Throws(()=>Simulator.Run(new int[5000],256,Policy.FIFO));
});
Test("Stack monotonicity", () =>
{
    var rng=new Random(13);
    for(int t=0;t<30;t++)
    {
        var refs=Enumerable.Range(0,50).Select(_=>rng.Next(8)).ToArray();
        foreach(var p in new[]{Policy.LRU,Policy.OPT})
        { int previous=int.MaxValue;for(int f=1;f<=8;f++){int current=Simulator.Run(refs,f,p,false).Faults;if(current>previous)throw new Exception("Fault count increased");previous=current;} }
    }
});
Test("100 random independent oracles and invariants", () =>
{
    var rng=new Random(2026);
    for(int t=0;t<100;t++)
    {
        int[] refs=Enumerable.Range(0,rng.Next(1,81)).Select(_=>rng.Next(9)).ToArray();int f=rng.Next(1,8);
        foreach(var p in Policies.All)
        {
            var r=Simulator.Run(refs,f,p);Equal(Oracle(refs,f,p),r.Faults);Equal(refs.Length,r.Hits+r.Faults);
            IReadOnlyList<int?> previous=new int?[f];
            foreach(var s in r.Steps)
            {
                var present=s.Frames.Where(x=>x.HasValue).ToArray();Equal(present.Length,present.Distinct().Count());
                Equal(true,present.Contains(s.Page));Equal(previous.Contains(s.Page),s.Hit);previous=s.Frames;
            }
            if(Simulator.Run(refs,f,Policy.OPT,false).Faults>r.Faults)throw new Exception("OPT exceeds policy faults");
        }
    }
});
Test("OPT exhaustive minimum 3276 cases", () =>
{
    int count=0;
    for(int n=1;n<=6;n++)
    for(int mask=0;mask<(int)Math.Pow(3,n);mask++)
    {
        int v=mask;var refs=new int[n];for(int i=0;i<n;i++){refs[i]=v%3;v/=3;}
        for(int f=1;f<=3;f++){Equal(Minimum(refs,f),Simulator.Run(refs,f,Policy.OPT,false).Faults);count++;}
    }
    Equal(3276,count);
});
Test("Stress 200000 resident hits", () => { int[] refs=Enumerable.Range(0,200000).Select(i=>i%17).ToArray();foreach(var p in Policies.All){var r=Simulator.Run(refs,32,p,false);Equal((17,0),(r.Faults,r.Replacements));} });
Test("Stress 200000 distinct misses", () => { int[] refs=Enumerable.Range(0,200000).ToArray();foreach(var p in Policies.All){var r=Simulator.Run(refs,32,p,false);Equal((200000,199968),(r.Faults,r.Replacements));} });
Test("Comparison and curve", () => {Equal(4,Simulator.Compare(textbook,3).Count);Sequence(Enumerable.Range(1,5),Simulator.Curve(textbook,5).Select(c=>c.Frames));Throws(()=>Simulator.Curve(textbook,33));});
Test("Reference parser", () => {Sequence(new[]{0,1,2,3},CsvFiles.ParseReferences("0, 1  2\n3"));foreach(string s in new[]{"","1,,2","1,","-1","1.2","x","2147483648"})Throws(()=>CsvFiles.ParseReferences(s));});
Test("CSV fixtures invalid", () => { foreach(string path in Directory.GetFiles(Path.Combine(root,"input","invalid"),"*.csv"))Throws(()=>CsvFiles.ReadInput(path)); });
string temp=Path.Combine(Path.GetTempPath(),"osvme13_"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(temp);
Test("CSV roundtrip BOM", () => {string path=Path.Combine(temp,"input.csv");CsvFiles.WriteInput(path,new InputCase([0,1,0],2));var r=CsvFiles.ReadInput(path);Equal(2,r.Frames);Sequence(new[]{0,1,0},r.Pages);});
Test("CSV malformed records", () =>
{
    foreach(string text in new[]{"","page,frames\n1,3\n","step,page,frames\n1,1,3,x\n","step,page,frames\n1,1.5,3\n","step,page,frames\n1,\"2,3\n","step,page,frames\n1,1,3\n\n"})
    {string path=Path.Combine(temp,"bad.csv");File.WriteAllText(path,text);Throws(()=>CsvFiles.ReadInput(path));}
    Sequence(new[]{"a,b","c\"d",""},CsvFiles.ParseRecord("\"a,b\",\"c\"\"d\","));
});
Test("CSV trace JSON and victim zero", () =>
{
    string path=Path.Combine(temp,"trace.csv");CsvFiles.WriteTrace(path,Simulator.Run([0,1,0],1,Policy.SecondChance));var lines=File.ReadAllLines(path);Equal(4,lines.Length);
    Equal("0",CsvFiles.ParseRecord(lines[2])[6]);var last=CsvFiles.ParseRecord(lines[3]);Equal("3",last[12]);Sequence(new int?[]{0},JsonSerializer.Deserialize<int?[]>(last[7])!);
    Throws(()=>CsvFiles.WriteTrace(path,Simulator.Run([1],1,Policy.FIFO,false)));
});
Test("Input expected textbook", () => {var r=CsvFiles.ReadInput(Path.Combine(root,"input","textbook.csv"));Equal(3,r.Frames);Sequence(textbook,r.Pages);});
Test("Snapshot immutability", () => {var r=Simulator.Run([1,2,3,4],3,Policy.FIFO);Equal<int?>(1,r.Steps[0].Frames[0]);Equal<int?>(null,r.Steps[0].Frames[1]);Equal<int?>(4,r.Steps[^1].Frames[0]);});

var rows=new List<object?[]>();int failures=0;
foreach(var (name,action) in tests)
{
    try{action();rows.Add([name,"PASS","All assertions satisfied"]);Console.WriteLine("PASS "+name);}
    catch(Exception ex){failures++;rows.Add([name,"FAIL",ex.Message]);Console.WriteLine("FAIL "+name+": "+ex.Message);}
}
Directory.Delete(temp,true);
CsvFiles.WriteRows(Path.Combine(root,"output","test_results.csv"),["test","status","detail"],rows);
File.WriteAllText(Path.Combine(root,"output","test_environment.json"),JsonSerializer.Serialize(new
{ utc=DateTimeOffset.UtcNow,runtime=RuntimeInformation.FrameworkDescription,os=RuntimeInformation.OSDescription,tests=tests.Count,failures,gui="Not part of cross-platform core runner; use WinForms --smoke on Windows" },new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"{tests.Count-failures}/{tests.Count} PASS. WinForms runtime is a separate Windows gate.");
return failures==0?0:1;

static int Oracle(int[] refs,int size,Policy policy)
{
    var memory=new List<int>();var queue=new List<int>();var last=new Dictionary<int,int>();var bits=new Dictionary<int,int>();int faults=0;
    for(int i=0;i<refs.Length;i++)
    {
        int p=refs[i];if(memory.Contains(p)){last[p]=i;bits[p]=1;continue;}faults++;
        if(memory.Count==size)
        {
            int victim;
            if(policy==Policy.FIFO){victim=queue[0];queue.RemoveAt(0);}
            else if(policy==Policy.LRU)victim=memory.MinBy(x=>last[x]);
            else if(policy==Policy.OPT)victim=memory.MaxBy(x=>{int next=Array.IndexOf(refs,x,i+1);return next<0?int.MaxValue:next;});
            else {while(bits[queue[0]]==1){int x=queue[0];queue.RemoveAt(0);bits[x]=0;queue.Add(x);}victim=queue[0];queue.RemoveAt(0);}
            memory.Remove(victim);
        }
        memory.Add(p);last[p]=i;bits[p]=1;if(policy is Policy.FIFO or Policy.SecondChance)queue.Add(p);
    }
    return faults;
}
static int Minimum(int[] refs,int size)
{
    var cache=new Dictionary<(int,int),int>();
    int Solve(int i,int mask)
    {
        if(i==refs.Length)return 0;if(cache.TryGetValue((i,mask),out int answer))return answer;
        int page=1<<refs[i];
        if((mask&page)!=0)answer=Solve(i+1,mask);
        else if(System.Numerics.BitOperations.PopCount((uint)mask)<size)answer=1+Solve(i+1,mask|page);
        else answer=1+Enumerable.Range(0,3).Where(v=>(mask&(1<<v))!=0).Min(v=>Solve(i+1,(mask&~(1<<v))|page));
        cache[(i,mask)]=answer;return answer;
    }
    return Solve(0,0);
}
