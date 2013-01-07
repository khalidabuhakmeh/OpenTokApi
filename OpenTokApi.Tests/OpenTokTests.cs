using System;
using System.Diagnostics;
using FluentAssertions;
using OpenTokApi.Core;
using Xunit;

namespace OpenTokApi.Tests
{
    public class OpenTokTests
    {
        protected OpenTok OpenTok { get; set; }
        private const string Localhost = "127.0.0.1";

        public OpenTokTests()
        {
            OpenTok = new OpenTok();
        }

        [Fact]
        public string Can_generate_a_session_with_no_options()
        {
            var session =  OpenTok.CreateSession(Localhost);
            session.Should().NotBeNullOrEmpty();
            Debug.WriteLine(string.Format("session : {0}", session));
            
            return session;
        }

        [Fact]
        public void Can_generate_a_session_with_p2p_disabled()
        {
            var session = OpenTok.CreateSession(Localhost, new { p2p_preference = "disabled" });
            session.Should().NotBeNullOrEmpty();
            Debug.WriteLine(string.Format("session : {0}", session));
        }

        [Fact]
        public void Can_generate_a_session_with_p2p_enabled()
        {
            var session = OpenTok.CreateSession(Localhost, new { p2p_preference = "enabled" });
            session.Should().NotBeNullOrEmpty();
            Debug.WriteLine(string.Format("session : {0}", session));
        }

        [Fact]
        public void Can_generate_a_token_with_no_options()
        {
            var session = Can_generate_a_session_with_no_options();
            var token = OpenTok.GenerateToken(session);

            token.Should().NotBeNullOrEmpty();
            Debug.WriteLine(string.Format("token : {0}", token));
        }

        [Fact]
        public void Can_generate_a_token_with_subscriber_role()
        {
            var session = Can_generate_a_session_with_no_options();
            var token = OpenTok.GenerateToken(session, new { role = Roles.Subscriber });

            token.Should().NotBeNullOrEmpty();
            Debug.WriteLine(string.Format("token : {0}", token));
        }

        [Fact]
        public void Can_generate_a_token_with_moderator_role()
        {
            var session = Can_generate_a_session_with_no_options();
            var token = OpenTok.GenerateToken(session, new { role = Roles.Moderator });

            token.Should().NotBeNullOrEmpty();
            Debug.WriteLine(string.Format("token : {0}", token));
        }

        [Fact]
        public void Can_generate_a_token_with_expire_time()
        {
            var session = Can_generate_a_session_with_no_options();
            var token = OpenTok.GenerateToken(session, new { expire_time = DateTime.Now.AddSeconds(30) });

            token.Should().NotBeNullOrEmpty();
            Debug.WriteLine(string.Format("token : {0}", token));
        }

        [Fact]
        public void Can_generate_a_token_with_connection_data()
        {
            var session = Can_generate_a_session_with_no_options();
            var token = OpenTok.GenerateToken(session, new { connection_data = "metadata describing the connection" });

            token.Should().NotBeNullOrEmpty();
            Debug.WriteLine(string.Format("token : {0}", token));
        }

    }
}
