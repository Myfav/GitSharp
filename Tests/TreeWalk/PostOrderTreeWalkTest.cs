/*
 * Copyright (C) 2008, Google Inc.
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or
 * without modification, are permitted provided that the following
 * conditions are met:
 *
 * - Redistributions of source code must retain the above copyright
 *   notice, this list of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above
 *   copyright notice, this list of conditions and the following
 *   disclaimer in the documentation and/or other materials provided
 *   with the distribution.
 *
 * - Neither the name of the Git Development Community nor the
 *   names of its contributors may be used to endorse or promote
 *   products derived from this software without specific prior
 *   written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System.IO;
using GitSharp.DirectoryCache;
using Xunit;

namespace GitSharp.Tests.TreeWalk
{
	public class PostOrderTreeWalkTest : RepositoryTestCase
	{
		[Fact]
		public void testInitialize_NoPostOrder()
		{
			var tw = new GitSharp.TreeWalk.TreeWalk(db);
			Assert.False(tw.PostOrderTraversal);
		}

		[Fact]
		public void testInitialize_TogglePostOrder()
		{
			var tw = new GitSharp.TreeWalk.TreeWalk(db);
			Assert.False(tw.PostOrderTraversal);
			tw.PostOrderTraversal = true;
			Assert.True(tw.PostOrderTraversal);
			tw.PostOrderTraversal = false;
			Assert.False(tw.PostOrderTraversal);
		}

		[Fact]
		public void testResetDoesNotAffectPostOrder()
		{
			var tw = new GitSharp.TreeWalk.TreeWalk(db) { PostOrderTraversal = true };
			Assert.True(tw.PostOrderTraversal);
			tw.reset();
			Assert.True(tw.PostOrderTraversal);

			tw.PostOrderTraversal = false;
			Assert.False(tw.PostOrderTraversal);
			tw.reset();
			Assert.False(tw.PostOrderTraversal);
		}

		[Fact]
		public void testNoPostOrder()
		{
			DirCache tree = DirCache.read(db);
			{
				DirCacheBuilder b = tree.builder();

				b.add(makeFile("a"));
				b.add(makeFile("b/c"));
				b.add(makeFile("b/d"));
				b.add(makeFile("q"));

				b.finish();
				Assert.Equal(4, tree.getEntryCount());
			}

			var tw = new GitSharp.TreeWalk.TreeWalk(db);
			tw.reset();
			tw.PostOrderTraversal = false;
			tw.addTree(new DirCacheIterator(tree));

			assertModes("a", FileMode.RegularFile, tw);
			assertModes("b", FileMode.Tree, tw);
			Assert.True(tw.isSubtree());
			Assert.False(tw.isPostChildren());
			tw.enterSubtree();
			assertModes("b/c", FileMode.RegularFile, tw);
			assertModes("b/d", FileMode.RegularFile, tw);
			assertModes("q", FileMode.RegularFile, tw);
		}

		[Fact]
		public void testWithPostOrder_EnterSubtree()
		{
			DirCache tree = DirCache.read(db);
			{
				DirCacheBuilder b = tree.builder();

				b.add(makeFile("a"));
				b.add(makeFile("b/c"));
				b.add(makeFile("b/d"));
				b.add(makeFile("q"));

				b.finish();
				Assert.Equal(4, tree.getEntryCount());
			}

			var tw = new GitSharp.TreeWalk.TreeWalk(db);
			tw.reset();
			tw.PostOrderTraversal = true;
			tw.addTree(new DirCacheIterator(tree));

			assertModes("a", FileMode.RegularFile, tw);

			assertModes("b", FileMode.Tree, tw);
			Assert.True(tw.isSubtree());
			Assert.False(tw.isPostChildren());
			tw.enterSubtree();
			assertModes("b/c", FileMode.RegularFile, tw);
			assertModes("b/d", FileMode.RegularFile, tw);

			assertModes("b", FileMode.Tree, tw);
			Assert.True(tw.isSubtree());
			Assert.True(tw.isPostChildren());

			assertModes("q", FileMode.RegularFile, tw);
		}

		[Fact]
		public void testWithPostOrder_NoEnterSubtree()
		{
			DirCache tree = DirCache.read(db);
			{
				DirCacheBuilder b = tree.builder();

				b.add(makeFile("a"));
				b.add(makeFile("b/c"));
				b.add(makeFile("b/d"));
				b.add(makeFile("q"));

				b.finish();
				Assert.Equal(4, tree.getEntryCount());
			}

			var tw = new GitSharp.TreeWalk.TreeWalk(db);
			tw.reset();
			tw.PostOrderTraversal = true;
			tw.addTree(new DirCacheIterator(tree));

			assertModes("a", FileMode.RegularFile, tw);

			assertModes("b", FileMode.Tree, tw);
			Assert.True(tw.isSubtree());
			Assert.False(tw.isPostChildren());

			assertModes("q", FileMode.RegularFile, tw);
		}

		private DirCacheEntry makeFile(string path)
		{
			byte[] pathBytes = Constants.encode(path);
			var ent = new DirCacheEntry(path);
			ent.setFileMode(FileMode.RegularFile);
			ent.setObjectId(new ObjectWriter(db).ComputeBlobSha1(pathBytes.Length, new MemoryStream(pathBytes)));
			return ent;
		}

		private static void assertModes(string path, FileMode mode0, GitSharp.TreeWalk.TreeWalk tw)
		{
			Assert.True(tw.next(), "has " + path);
			Assert.Equal(path, tw.getPathString());
			Assert.Equal(mode0, tw.getFileMode(0));
		}

	}
}