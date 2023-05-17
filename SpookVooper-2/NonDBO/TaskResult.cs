using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV2.NonDBO;

public abstract class ITaskResult
{
	public string Message { get; set; }
	public bool Success { get; set; }
}

public class TaskResult : ITaskResult
{

}

public class PurchaseLandTaskResult : ITaskResult
{
	public Dictionary<string, decimal> TotalCost { get; set; }
	public string Name { get; set; }
}