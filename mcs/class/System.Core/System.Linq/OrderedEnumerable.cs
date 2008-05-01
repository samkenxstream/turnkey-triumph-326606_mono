//
// OrderedEnumerable.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Linq {

	abstract class OrderedEnumerable<TElement> : IOrderedEnumerable<TElement> {

		protected OrderedEnumerable<TElement> parent;

		public abstract IEnumerator<TElement> GetEnumerator ();
		public abstract IEnumerable<TElement> Sort (IEnumerable<TElement> parentSource);

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
                        
		public IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey> (
			Func<TElement, TKey> selector, IComparer<TKey> comparer, bool descending)
		{
			parent = new OrderedSequence<TElement, TKey> (this, selector, comparer, descending);
			return parent;
		}
	}
}
