//
// System.Web.StaticSiteMapProvider.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Juraj Skripsky (js@hotfeet.ch)
//
// (C) 2003 Ben Maurer
// (C) 2005 Novell, Inc (http://www.novell.com)
// (C) 2007 HotFeet GmbH (http://www.hotfeet.ch)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


#if NET_2_0
using System.Collections.Generic;

namespace System.Web
{
	public abstract class StaticSiteMapProvider : SiteMapProvider
	{
		Dictionary<string, SiteMapNode> keyToNode;
		Dictionary<SiteMapNode, SiteMapNode> nodeToParent;
		Dictionary<SiteMapNode, SiteMapNodeCollection> nodeToChildren;
		Dictionary<string, SiteMapNode> urlToNode;
			
		public StaticSiteMapProvider ()
		{
			keyToNode = new Dictionary<string, SiteMapNode> ();
			nodeToParent = new Dictionary<SiteMapNode, SiteMapNode> ();
			nodeToChildren = new Dictionary<SiteMapNode, SiteMapNodeCollection> ();
			urlToNode = new Dictionary<string, SiteMapNode> (StringComparer.InvariantCultureIgnoreCase);
		}

		internal protected override void AddNode (SiteMapNode node, SiteMapNode parentNode)
		{
			if (node == null)
				throw new ArgumentNullException ("node");

			lock (this_lock) {
				if (FindSiteMapNodeFromKey (node.Key) != null)
					throw new InvalidOperationException (string.Format ("A node with key '{0}' already exists.",node.Key));

				if (!String.IsNullOrEmpty (node.Url)) {
					string url = MapUrl (node.Url);
					
					if (FindSiteMapNode (url) != null)
						throw new InvalidOperationException (String.Format (
							"Multiple nodes with the same URL '{0}' were found. " + 
							"StaticSiteMapProvider requires that sitemap nodes have unique URLs.",
							node.Url
						));
				
					urlToNode.Add (url, node);
				}
				keyToNode.Add (node.Key, node);

				if (node == RootNode)
					return;

				if (parentNode == null)
					parentNode = RootNode;

				nodeToParent.Add (node, parentNode);

				SiteMapNodeCollection children;
				if (!nodeToChildren.TryGetValue (parentNode, out children)) 
					nodeToChildren.Add (parentNode, children = new SiteMapNodeCollection ());

				children.Add (node);
			}
		}
		
		protected virtual void Clear ()
		{
			lock (this_lock) {
				urlToNode.Clear ();
				nodeToChildren.Clear ();
				nodeToParent.Clear ();
				keyToNode.Clear ();
			}
		}

		public override SiteMapNode FindSiteMapNode (string rawUrl)
		{
			if (rawUrl == null)
				throw new ArgumentNullException ("rawUrl");
			
			if (rawUrl == String.Empty)
				return null;			
			
			BuildSiteMap();
			SiteMapNode node;
			urlToNode.TryGetValue (MapUrl (rawUrl), out node);
			return CheckAccessibility (node);
		}

		public override SiteMapNodeCollection GetChildNodes (SiteMapNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			
			BuildSiteMap();
			SiteMapNodeCollection col;
			if (!nodeToChildren.TryGetValue (node, out col))
				return SiteMapNodeCollection.EmptyCollection;
			
			SiteMapNodeCollection ret = null;
			for (int n=0; n<col.Count; n++) {
				if (!IsAccessibleToUser (HttpContext.Current, col[n])) {
					if (ret == null) {
						ret = new SiteMapNodeCollection ();
						for (int m=0; m<n; m++)
							ret.Add (col[m]);
					}
				} else if (ret != null)
					ret.Add (col[n]);
			}

			if (ret == null)
				return SiteMapNodeCollection.ReadOnly (col);
			else if (ret.Count > 0)
				return SiteMapNodeCollection.ReadOnly (ret);
			else
				return SiteMapNodeCollection.EmptyCollection;
		}
		
		public override SiteMapNode GetParentNode (SiteMapNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");

			BuildSiteMap();
			SiteMapNode parent;
			nodeToParent.TryGetValue (node, out parent);
			return CheckAccessibility (parent);
		}
		
		protected override void RemoveNode (SiteMapNode node)
		{
			if (node == null)
				throw new ArgumentNullException("node");
			
			lock (this_lock) {
				keyToNode.Remove (node.Key);
				if (!String.IsNullOrEmpty (node.Url))
					urlToNode.Remove (MapUrl (node.Url));

				if (node == RootNode)
					return;

				SiteMapNode parent = nodeToParent [node];
				nodeToParent.Remove (node);
				nodeToChildren [parent].Remove (node);
			}
		}
		
		public override SiteMapNode FindSiteMapNodeFromKey (string key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			
			SiteMapNode ret;
			keyToNode.TryGetValue (key, out ret);
			return CheckAccessibility (ret);
		}

		public abstract SiteMapNode BuildSiteMap ();
		
		SiteMapNode CheckAccessibility (SiteMapNode node) {
			return (node != null && IsAccessibleToUser (HttpContext.Current, node)) ? node : null;
		}

		string MapUrl (string url)
		{
			if (VirtualPathUtility.IsAppRelative (url))
				return VirtualPathUtility.ToAbsolute (url);
			else
				return url;
		}
	}
}
#endif

