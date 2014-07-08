/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

using System;
using System.Text;
using System.Collections.Generic;
using QSharp.String.Stream;


namespace QSharp.String.Compiler
{
    public class SyntaxTree
    {
        #region Nested classes 

        public class NodeBase : IDisposable   
        {
            public NodeNonterminal Parent = null;
            public int IOfParent = 0;

            public void Dispose()
            {
                GC.SuppressFinalize(this);  
                    /* tell the garbage collection not to call the destructor hereafter */
            }

            public NodeBase LeftmostLeaf
            {
                get
                {
                    var node = this;
                    while (true)
                    {
                        if (node is NodeNonterminal)
                        {
                            var ntnode = (NodeNonterminal)node;
                            if (ntnode.NSubnodes > 0)
                            {
                                node = ntnode[0];
                            }
                            else
                            {
                                return node;
                            }
                        }
                        else
                        {
                            return node;
                        }
                    }
                }
            }

            public NodeBase NextLeaf
            {
                get
                {
                    NodeBase node = this;
                    for (NodeNonterminal parent = Parent; parent != null; node = parent, parent = node.Parent)
                    {
                        if (node.IOfParent < parent.NSubnodes - 1)
                        {
                            node = parent[node.IOfParent+1].LeftmostLeaf;
                            return node;
                        }
                    }
                    return null;
                }
            }

            public NodeBase NextStub
            {
                get
                {
                    NodeBase node = this;
                    for (NodeNonterminal parent = Parent; parent != null; node = parent, parent = node.Parent)
                    {
                        if (node.IOfParent < parent.NSubnodes - 1)
                        {
                            return parent[node.IOfParent+1];
                        }
                    }
                    return null;
                }
            }

            public int Depth
            {
                get
                {
                    int nDepth = 1;
                    for (NodeNonterminal parent = Parent; parent != null; parent = parent.Parent)
                    {
                        nDepth++;
                    }
                    return nDepth;
                }
            }
        }

        public class NodeNonterminal : NodeBase
        {
            public readonly Bnf.Nonterminal Ref = new Bnf.Nonterminal();
            public readonly List<NodeBase> Subnodes = new List<NodeBase>();

            public NodeNonterminal()
            {
            }

            public NodeNonterminal(Bnf.Nonterminal n)
            {
                Ref = n;
            }

            public NodeBase this[int index]
            {
                get { return Subnodes[index]; }
            }

            public int NSubnodes
            {
                get { return Subnodes.Count; }
            }

            public void RemoveAsSubtree()
            {
                Parent.Subnodes.RemoveAt(IOfParent);
                // FIXME: the node should be disposed at once
                // for performance
            }
            
            public void RemoveAllSubnodes()
            {
                Subnodes.Clear();
            }

            public void InsertSubnode(int iPos, NodeBase node)
            {
                Subnodes.Insert(iPos, node);
                node.Parent = this;
                // update IOfParent of offsprings
                for (var i = iPos; i < Subnodes.Count; i++)
                {
                    Subnodes[i].IOfParent = i;
                }
            }

            public void AddSubnode(NodeBase node)
            {
                Subnodes.Add(node);
                node.Parent = this;
                node.IOfParent = Subnodes.Count - 1;
            }

            public void Produce(Bnf.IPhrase phrase)
            {
                Subnodes.Clear();
                foreach (var symbol in phrase)
                {
                    var nont = symbol as Bnf.Nonterminal;
                    if (nont != null)
                    {
                        AddSubnode(new NodeNonterminal(nont));
                    }
                    else
                    {
                        AddSubnode(new NodeTerminal((Bnf.Terminal)symbol));
                    }
                }
            }

            /**
             * SyntaxTree.NodeNonterminal.CleanupTentativeNodes
             * <summary>
             *  Clean up tentative nodes on the right. i.e. 
             *  remove all the nodes preorderly succeeding the specified node
             *  except those whose parent is on the path from the node to 
             *  the root, so that all the forebearers of the node should have
             *  3trees on the right of the path with a depth of no more than 
             *  1 level (with no child)
             * </summary>
             */
            public void CleanupTentativeNodes()
            {
                var node = this;
                node.Subnodes.Clear();
                var parent = node.Parent;
                for ( ; parent != null; node = parent, parent = node.Parent)
                {
                    var iOfParent = node.IOfParent;
                    for (int i = iOfParent + 1; i < parent.Subnodes.Count; i++)
                    {
                        var subnode = parent.Subnodes[i];
                        var nont = subnode as NodeNonterminal;
                        if (nont != null)
                        {
                            nont.Subnodes.Clear();
                        }
                    }
                }
            }

            public override string ToString()
            {
                string s = "NodeNonterminal { Ref.Index = " + Ref.Index;
                s += "; Subnodes.Count = " + Subnodes.Count + " }";
                return s;
            }
        }

        public class NodeTerminal : NodeBase
        {
            public readonly Bnf.Terminal Ref = null;
            public TokenStream.Position Pos = null;

            public NodeTerminal()
            {
            }

            public NodeTerminal(Bnf.Terminal t)
            {
                Ref = t;
            }

            public override string ToString()
            {
                return new StringBuilder("NodeTerminal { Ref = ")
                    .Append(Ref)
                    .Append(" }").ToString();
            }
        }

        #endregion

        #region Fields

        public NodeNonterminal Root = new NodeNonterminal();

        #endregion

        protected string SubtreeToString(NodeBase node, uint nDepth)
        {
            var leadingWs = Utility.MakeWhitespaces(nDepth * 2);
            var sb = new StringBuilder(leadingWs);

            var nodeNt = node as NodeNonterminal;
            if (nodeNt != null)
            {
                sb.Append("NodeNonterminal { Ref.Index = ").Append(nodeNt.Ref.Index).Append("; ");
                sb.Append("Subnodes.Count = ").Append(nodeNt.Subnodes.Count).Append("; Subnodes: \r\treesize");

                foreach (var subnode in nodeNt.Subnodes)
                {
                    sb.Append(SubtreeToString(subnode, nDepth + 1));
                    sb.Append("\r\treesize");
                }

                sb.Append(leadingWs).Append("}");
            }
            else
            {   // node is terminal
                sb.Append(node);
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            return SubtreeToString(Root, 0);
        }
    }

#if TEST_String_Compiler_SyntaxTree
    class SyntaxTreeTest
    {
        public static void Main(string[] args)
        {
            //SyntaxTree tree = new SyntaxTree();
            SyntaxTree.NodeBase node1 = new SyntaxTree.NodeNonterminal();
            SyntaxTree.NodeBase node2 = new SyntaxTree.NodeTerminal();

            Console.WriteLine("{0}\n", node1 is SyntaxTree.NodeTerminal);
            Console.WriteLine("{0}\n", node2 is SyntaxTree.NodeTerminal);

            ((SyntaxTree.NodeNonterminal)node1).AddSubnode(node2);
        }
    }
#endif
}   /* namespace QSharp.String.Compiler */
