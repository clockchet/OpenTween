﻿// OpenTween - Client of Twitter
// Copyright (c) 2013 kim_upsilon (@kim_upsilon) <https://upsilo.net/~upsilon/>
// All rights reserved.
//
// This file is part of OpenTween.
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option)
// any later version.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License
// for more details.
//
// You should have received a copy of the GNU General Public License along
// with this program. If not, see <http://www.gnu.org/licenses/>, or write to
// the Free Software Foundation, Inc., 51 Franklin Street - Fifth Floor,
// Boston, MA 02110-1301, USA.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Extensions;

namespace OpenTween
{
    public class TwitterTest
    {
        [Theory]
        [InlineData("https://twitter.com/twitterapi/status/22634515958",
            new[] { "22634515958" })]
        [InlineData("<a target=\"_self\" href=\"https://t.co/aaaaaaaa\" title=\"https://twitter.com/twitterapi/status/22634515958\">twitter.com/twitterapi/stat…</a>",
            new[] { "22634515958" })]
        [InlineData("<a target=\"_self\" href=\"https://t.co/bU3oR95KIy\" title=\"https://twitter.com/haru067/status/224782458816692224\">https://t.co/bU3oR95KIy</a>" +
            "<a target=\"_self\" href=\"https://t.co/bbbbbbbb\" title=\"https://twitter.com/karno/status/311081657790771200\">https://t.co/bbbbbbbb</a>",
            new[] { "224782458816692224", "311081657790771200" })]
        [InlineData("https://mobile.twitter.com/muji_net/status/21984934471",
            new[] { "21984934471" })]
        [InlineData("https://twitter.com/imgazyobuzi/status/293333871171354624/photo/1",
            new[] { "293333871171354624" })]
        public void StatusUrlRegexTest(string url, string[] expected)
        {
            var results = Twitter.StatusUrlRegex.Matches(url).Cast<Match>()
                .Select(x => x.Groups["StatusId"].Value).ToArray();

            Assert.Equal(expected, results);
        }

        [Theory]
        [InlineData("http://favstar.fm/users/twitterapi/status/22634515958", new[] { "22634515958" })]
        [InlineData("http://ja.favstar.fm/users/twitterapi/status/22634515958", new[] { "22634515958" })]
        [InlineData("http://favstar.fm/t/22634515958", new[] { "22634515958" })]
        [InlineData("http://aclog.koba789.com/i/312485321239564288", new[] { "312485321239564288" })]
        [InlineData("http://frtrt.net/solo_status.php?status=263483634307198977", new[] { "263483634307198977" })]
        public void ThirdPartyStatusUrlRegexTest(string url, string[] expected)
        {
            var results = Twitter.ThirdPartyStatusUrlRegex.Matches(url).Cast<Match>()
                .Select(x => x.Groups["StatusId"].Value).ToArray();

            Assert.Equal(expected, results);
        }

        [Fact]
        public void FindTopOfReplyChainTest()
        {
            var posts = new Dictionary<long, PostClass>
            {
                {950L, new PostClass { StatusId = 950L, InReplyToStatusId = null }}, // このツイートが末端
                {987L, new PostClass { StatusId = 987L, InReplyToStatusId = 950L }},
                {999L, new PostClass { StatusId = 999L, InReplyToStatusId = 987L }},
                {1000L, new PostClass { StatusId = 1000L, InReplyToStatusId = 999L }},
            };
            Assert.Equal(950L, Twitter.FindTopOfReplyChain(posts, 1000L).StatusId);
            Assert.Equal(950L, Twitter.FindTopOfReplyChain(posts, 950L).StatusId);
            Assert.Throws<ArgumentException>(() => Twitter.FindTopOfReplyChain(posts, 500L));

            posts = new Dictionary<long, PostClass>
            {
                // 1200L は posts の中に存在しない
                {1210L, new PostClass { StatusId = 1210L, InReplyToStatusId = 1200L }},
                {1220L, new PostClass { StatusId = 1220L, InReplyToStatusId = 1210L }},
                {1230L, new PostClass { StatusId = 1230L, InReplyToStatusId = 1220L }},
            };
            Assert.Equal(1210L, Twitter.FindTopOfReplyChain(posts, 1230L).StatusId);
            Assert.Equal(1210L, Twitter.FindTopOfReplyChain(posts, 1210L).StatusId);
        }
    }
}
