using Xunit;
using TestStack.BDDfy;
using Shouldly;
using Rafty.Concensus;
using System;
using System.Collections.Generic;
using Rafty.FiniteStateMachine;
using Rafty.Log;

namespace Rafty.UnitTests
{
    public class AllServersApplyToStateMachineTests
    {
/*
• If commitIndex > lastApplied: increment lastApplied, apply log[lastApplied] to state machine (§5.3)\
*/
        private List<IPeer> _peers;
        private readonly ILog _log;
        private readonly IRandomDelay _random;
        private readonly INode _node;
        private IFiniteStateMachine _fsm;

        public AllServersApplyToStateMachineTests()
        {
            _random = new RandomDelay();
            _peers = new List<IPeer>();
            _log = new InMemoryLog();
            _fsm = new InMemoryStateMachine();
            _node = new NothingNode();
        }

        [Fact] 
        public void FollowerShouldApplyLogsToFsm()
        {
            var currentState = new CurrentState(Guid.NewGuid(), 0, default(Guid), 0, 0);
            var fsm = new InMemoryStateMachine();
            var follower = new Follower(currentState, fsm, _log, _random, _node, new SettingsBuilder().Build());
            var log = new LogEntry("test", typeof(string), 1);
            var appendEntries = new AppendEntriesBuilder()
                .WithTerm(1)
                .WithPreviousLogTerm(1)
                .WithLeaderCommitIndex(1)
                .WithPreviousLogIndex(1)
                .WithEntry(log)
                .Build();
            //assume node has added the log..
            _log.Apply(log);
            var appendEntriesResponse = follower.Handle(appendEntries);
            follower.CurrentState.CurrentTerm.ShouldBe(1);
            follower.CurrentState.LastApplied.ShouldBe(1);
            fsm.ExposedForTesting.ShouldBe(1);
        }

        [Fact] 
        public void CandidateShouldApplyLogsToFsm()
        {
            var currentState = new CurrentState(Guid.NewGuid(), 0, default(Guid), 0, 0);
            var fsm = new InMemoryStateMachine();
            var candidate = new Candidate(currentState,fsm, _peers, _log, _random, _node, new SettingsBuilder().Build());
            var log = new LogEntry("test", typeof(string), 1);
            var appendEntries = new AppendEntriesBuilder()
                .WithTerm(1)
                .WithPreviousLogTerm(1)
                .WithEntry(log)
                .WithPreviousLogIndex(1)
                .WithLeaderCommitIndex(1)
                .Build();
            //assume node has added the log..
            _log.Apply(log);
            var appendEntriesResponse = candidate.Handle(appendEntries);
            candidate.CurrentState.CurrentTerm.ShouldBe(1);
            candidate.CurrentState.LastApplied.ShouldBe(1);
            fsm.ExposedForTesting.ShouldBe(1);
            var node = (NothingNode) _node;
            node.BecomeFollowerCount.ShouldBe(1);
        }


        [Fact] 
        public void LeaderShouldApplyLogsToFsm()
        {
            
            var currentState = new CurrentState(Guid.NewGuid(), 0, default(Guid), 0, 0);
            var fsm = new InMemoryStateMachine();
            var leader = new Leader(currentState, fsm, _peers, _log, _node, new SettingsBuilder().Build());
            var log = new LogEntry("test", typeof(string), 1);
               var appendEntries = new AppendEntriesBuilder()
                .WithTerm(1)
                .WithPreviousLogTerm(1)
                .WithEntry(log)
                .WithPreviousLogIndex(1)
                .WithLeaderCommitIndex(1)
                .Build();
            //assume node has added the log..
            _log.Apply(log);
            var appendEntriesResponse = leader.Handle(appendEntries);
            leader.CurrentState.CurrentTerm.ShouldBe(1);
            leader.CurrentState.LastApplied.ShouldBe(1);
            fsm.ExposedForTesting.ShouldBe(1);
        }
    }
}