using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1;
public class Polytechnics
{
    public string Name { get; set; } = "";
    public List<string?> Webpage { get; set; } = new(); // se for só string vai dar erro porque aquilo é um array de strings
}
