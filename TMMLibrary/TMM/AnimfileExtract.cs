using System.Xml;

namespace TMMLibrary.TMM;

/// <summary>
/// Reads data from an XML file where the root node is <c>animfile</c>. This is not a full representation
/// of an "animfile", just useful to extract the relevant paths for resolving texture and model data. 
/// </summary>
public class AnimfileExtract : IDisposable
{
    public List<string> TmModelFiles = [];
    public List<string> TmAnimFiles = [];

    private XmlReader rd;
    private int state;
    private List<string> stack;

    public AnimfileExtract(Stream source)
    {
        rd = XmlReader.Create(source);
        state = 0;
        stack = [];
    }

    public void Decode()
    {
        while (rd.Read())
        {
            switch (rd.NodeType)
            {
                case XmlNodeType.Element:
                    stack.Add(rd.Name);
                    if (rd.Name == "assetreference")
                    {
                        var assetType = rd.GetAttribute("type");
                        state = assetType switch
                        {
                            "TMModel" => 1,
                            "TMAnimation" => 2,
                            _ => throw new DecodeException(this, $"assetreference unknown type {assetType}")
                        };
                    }
                    break;
                case XmlNodeType.EndElement:
                    stack.RemoveAt(stack.Count - 1);
                    break;
                case XmlNodeType.Text:
                    if (StackEndsWith(["assetreference", "file"]))
                    {
                        switch (state)
                        {
                            case 1:
                                TmModelFiles.Add(rd.Value);
                                break;
                            case 2:
                                TmAnimFiles.Add(rd.Value);
                                break;
                        }
                        state = 0;
                    }
                    break;
                case XmlNodeType.Attribute:
                    break;
            }
        }
    }

    private bool StackEndsWith(IEnumerable<string> parts)
    {
        var i = stack.Count - 1;
        foreach (var item in parts.Reverse())
        {
            if (item != stack[i])
            {
                return false;
            }
            i--;
        }
        return true;
    }

    public void Dispose()
    {
        rd.Dispose();
    }
}