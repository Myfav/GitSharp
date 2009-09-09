﻿/*
 * Copyright (C) 2007, Dave Watson <dwatson@mimvista.com>
 * Copyright (C) 2008, Robin Rosenberg <robin.rosenberg@dewire.com>
 * Copyright (C) 2008, Shawn O. Pearce <spearce@spearce.org>
 * Copyright (C) 2008, Marek Zawirski <marek.zawirski@gmail.com>
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

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace GitSharp.Tests
{
    public class RepositoryConfigTest
    {
        [Fact]
        public void ReadBareKey()
        {
            Config c = parse("[foo]\nbar\n");
            Assert.Equal(true, c.getBoolean("foo", null, "bar", false));
            Assert.Equal("", c.getString("foo", null, "bar"));
        }

        [Fact]
        public void ReadWithSubsection()
        {
            Config c = parse("[foo \"zip\"]\nbar\n[foo \"zap\"]\nbar=false\nn=3\n");
            Assert.Equal(true, c.getBoolean("foo", "zip", "bar", false));
            Assert.Equal("", c.getString("foo", "zip", "bar"));
            Assert.Equal(false, c.getBoolean("foo", "zap", "bar", true));
            Assert.Equal("false", c.getString("foo", "zap", "bar"));
            Assert.Equal(3, c.getInt("foo", "zap", "n", 4));
            Assert.Equal(4, c.getInt("foo", "zap", "m", 4));
        }

        [Fact]
        public void PutRemote()
        {
            Config c = new Config();
            c.setString("sec", "ext", "name", "value");
            c.setString("sec", "ext", "name2", "value2");
            string expText = "[sec \"ext\"]\n\tname = value\n\tname2 = value2\n";
            Assert.Equal(expText, c.toText());
        }

        [Fact]
        public void PutGetSimple()
        {
            Config c = new Config();
            c.setString("my", null, "somename", "false");
            Assert.Equal("false", c.getString("my", null, "somename"));
            Assert.Equal("[my]\n\tsomename = false\n", c.toText());
        }

        [Fact]
        public void PutGetStringList()
        {
            Config c = new Config();
            List<string> values = new List<string>();
            values.Add("value1");
            values.Add("value2");
            c.setStringList("my", null, "somename", values);

            object[] expArr = values.ToArray();
            string[] actArr = c.getStringList("my", null, "somename");
            Assert.True(expArr.SequenceEqual(actArr));

            string expText = "[my]\n\tsomename = value1\n\tsomename = value2\n";
            Assert.Equal(expText, c.toText());
        }

        [Fact]
        public void ReadCaseInsensitive()
        {
            Config c = parse("[Foo]\nBar\n");
            Assert.Equal(true, c.getBoolean("foo", null, "bar", false));
            Assert.Equal("", c.getString("foo", null, "bar"));
        }

        [Fact]
        public void ReadBooleanTrueFalse1()
        {
            Config c = parse("[s]\na = true\nb = false\n");
            Assert.Equal("true", c.getString("s", null, "a"));
            Assert.Equal("false", c.getString("s", null, "b"));

            Assert.True(c.getBoolean("s", "a", false));
            Assert.False(c.getBoolean("s", "b", true));
        }

        [Fact]
        public void ReadLong()
        {
            assertReadLong(1L);
            assertReadLong(-1L);
            assertReadLong(long.MinValue);
            assertReadLong(long.MaxValue);
            assertReadLong(4L * 1024 * 1024 * 1024, "4g");
            assertReadLong(3L * 1024 * 1024, "3 m");
            assertReadLong(8L * 1024, "8 k");

            try
            {
                assertReadLong(-1, "1.5g");
                Assert.False(true, "incorrectly accepted 1.5g");
            }
            catch (ArgumentException e)
            {
                Assert.Equal("Invalid long value: s.a=1.5g", e.Message);
            }
        }

        private void assertReadLong(long exp)
        {
            assertReadLong(exp, exp.ToString());
        }

        private void assertReadLong(long exp, string act)
        {
            Config c = parse("[s]\na = " + act + "\n");
            Assert.Equal(exp, c.getLong("s", null, "a", 0L));
        }

        private Config parse(string content)
        {
            Config c = new Config(null);
            c.fromText(content);
            return c;
        }
    }

}