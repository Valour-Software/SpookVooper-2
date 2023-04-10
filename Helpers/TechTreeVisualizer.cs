using SV2.Scripting.LuaObjects;
using SV2.Helpers.TreeHelper;
using System.Drawing;
using SV2.Database.Models.Districts;
using SV2.Scripting;

namespace SV2.Helpers;

// large portions of this code came from https://rachel53461.wordpress.com/2014/04/20/algorithm-for-drawing-trees/

public class TechTreeVisualizer
{
    private const int NODE_HEIGHT = 60;
    private const int NODE_WIDTH = 200;
    private const int NODE_MARGIN_X = 50;
    private const int NODE_MARGIN_Y = 40;

    public TreeNodeModel<LuaResearch> _tree;
    public void Generate(LuaResearchCategory category)
    {
        var items = GameDataManager.BaseResearchObjsUnWraped.Values.ToList();
        var t = new LuaResearch()
        {
            Id = category.Id,
            ParentId = string.Empty,
            Name = category.Name + " Category",
            LuaResearchPrototype = new()
            {
                Color = category.Children.First().Color
            }
        };
        items.Add(t);
        _tree = new TreeNodeModel<LuaResearch>(t, null);
        _tree.Children = new() { GetSampleTree(items) };
        TreeHelpers<LuaResearch>.CalculateNodePositions(_tree);

        CalculateControlSize();
    }

    public string GenerateTooltip(LuaResearch research)
    {
        string html = $@"<span class='modifier-tooltip-name'><b>{research.Name}</b></span>";
        var state = new ExecutionState(null, null);
        if (research.LuaResearchPrototype is not null && research.LuaResearchPrototype.ModifierNodes is not null)
        {
            foreach (var item in research.LuaResearchPrototype.ModifierNodes)
            {
                html += "<br/>" + item.GenerateHTMLForListing(state);
            }
        }
        return html;
    }

    public string GenerateRect(int x, int y, int width, int height, string color, LuaResearch research)
    {
        return $@"<rect x='{x}px' y='{y}px' width='{width}px' height='{height}px' style='fill: #{color};' data-bs-toggle=""tooltip"" data-bs-html=""true"" data-bs-custom-class=""modifier-tooltip-div"" data-bs-sanitize=""false"" data-bs-title=""{GenerateTooltip(research)}""></rect>";
    }

    public string GenerateText(string text, int x, int y)
    {
        //return $@"<text x='{x}px' y='{y}px'><tspan x='{x}' dy='25'>{text}</tspan></text>";
        return $@"<text x='{x}px' y='{y}px' dominant-baseline='middle' text-anchor='middle'>{text}</text>";
    }

    public string GenerateLine(int x1, int y1, int x2, int y2, bool arrowhead, bool fliparrowhead)
    {
        var lastpart = arrowhead ? (fliparrowhead ? "marker-start='url(#arrowHead)'" : "marker-end='url(#arrowHead)'") : "";
        return $@"<line x1='{x1}px' y1='{y1}px' x2='{x2}px' y2='{y2}px' {lastpart}></line>";
    }

    public string DrawNode(TreeNodeModel<LuaResearch> node)
    {
        // rectangle where node will be positioned
        var nodeRect = new Rectangle(
            Convert.ToInt32(NODE_MARGIN_X + (node.X * (NODE_WIDTH + NODE_MARGIN_X))),
            NODE_MARGIN_Y + (node.Y * (NODE_HEIGHT + NODE_MARGIN_Y))
            , NODE_WIDTH, NODE_HEIGHT);

        var nodeRectHtml = GenerateRect(nodeRect.X, nodeRect.Y, nodeRect.Width, nodeRect.Height, node.Item.LuaResearchPrototype.Color, node.Item);

        nodeRectHtml += GenerateText(node.Item.Name, nodeRect.X + (nodeRect.Width / 2), nodeRect.Y + (nodeRect.Height / 2));

        // draw line to parent
        if (node.Parent != null)
        {
            var nodeTopMiddle = new Point(nodeRect.X + (nodeRect.Width / 2), nodeRect.Y);
            nodeRectHtml += GenerateLine(nodeTopMiddle.X, nodeTopMiddle.Y, nodeTopMiddle.X, nodeTopMiddle.Y - (NODE_MARGIN_Y / 2), true, true);
        }

        // draw line to children
        if (node.Children.Count > 0)
        {
            var nodeBottomMiddle = new Point(nodeRect.X + (nodeRect.Width / 2), nodeRect.Y + nodeRect.Height);
            nodeRectHtml += GenerateLine(nodeBottomMiddle.X, nodeBottomMiddle.Y, nodeBottomMiddle.X, nodeBottomMiddle.Y + (NODE_MARGIN_Y / 2), false, false);

                // draw line over children
            if (node.Children.Count > 1)
            {
                var childrenLineStart = new Point(
                    Convert.ToInt32(NODE_MARGIN_X + (node.GetRightMostChild().X * (NODE_WIDTH + NODE_MARGIN_X)) + (NODE_WIDTH / 2)),
                    nodeBottomMiddle.Y + (NODE_MARGIN_Y / 2));
                var childrenLineEnd = new Point(
                    Convert.ToInt32(NODE_MARGIN_X + (node.GetLeftMostChild().X * (NODE_WIDTH + NODE_MARGIN_X)) + (NODE_WIDTH / 2)),
                    nodeBottomMiddle.Y + (NODE_MARGIN_Y / 2));

                nodeRectHtml += GenerateLine(childrenLineStart.X, childrenLineStart.Y, childrenLineEnd.X, childrenLineEnd.Y, false, false);
            }
        }

        foreach (var item in node.Children)
        {
            nodeRectHtml += DrawNode(item);
        }

        return nodeRectHtml;
    }

    private void CalculateControlSize()
    {
        // tree sizes are 0-based, so add 1
        var treeWidth = _tree.Width + 1;
        var treeHeight = _tree.Height + 1;
    }

    private static List<TreeNodeModel<LuaResearch>> GetChildNodes(List<LuaResearch> data, TreeNodeModel<LuaResearch> parent)
    {
        var nodes = new List<TreeNodeModel<LuaResearch>>();

        foreach (var item in data.Where(x => x.ParentId == parent.Item.Id))
        {
            var treeNode = new TreeNodeModel<LuaResearch>(item, parent);
            treeNode.Children = GetChildNodes(data, treeNode);
            nodes.Add(treeNode);
        }

        return nodes;
    }
    // converts list of sample items to hierarchial list of TreeNodeModels
    private TreeNodeModel<LuaResearch> GetSampleTree(List<LuaResearch> data)
    {
        var root = data.FirstOrDefault(p => p.ParentId == string.Empty);
        //var rootparent = new TreeNodeModel<LuaResearch>(new() { Id = null}, null);
        var rootTreeNode = new TreeNodeModel<LuaResearch>(root, null);

        // add tree node children recursively
        rootTreeNode.Children = GetChildNodes(data, rootTreeNode);

        return rootTreeNode;
    }
}
