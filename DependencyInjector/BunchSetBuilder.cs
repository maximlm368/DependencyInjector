using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;

namespace DependencyResolver
{
    class BunchSetBuilder
    {
        private class DataForCircuitRandering
        {
            public int _beingProcessedNodeNumber = 0;

            public List<DependencyCircuit> _relevantBunch = null;

            public List<DependencyCircuit> _obsoleteBunch = null;

            public bool _metSomeTop = false;

            public bool _canRegesterBunch = true;

            public bool _circuitContinues = true;

            public bool _isContinuationAfterFork = false;
        }


        private Dictionary<ParamNode , List<DependencyCircuit>> _nodeToCircuits;

        private Dictionary<List<DependencyCircuit> , Bunch> _circuitsToBunch;

        private List<DependencyCircuit> _circuits;

        private static string _nullArg = "'circuits' param in ctor can not be null";


        internal BunchSetBuilder ( List<DependencyCircuit> circuits,   Dictionary <ParamNode, List <DependencyCircuit>> nodeToCircuits,
                                                                                            Dictionary <List <DependencyCircuit>, Bunch> circuitsToBunch )
        {
            _nodeToCircuits = nodeToCircuits ?? throw new ArgumentNullException ( _nullArg );
            _circuitsToBunch = circuitsToBunch ?? throw new ArgumentNullException ( _nullArg );
            _circuits = circuits ?? throw new ArgumentNullException ( _nullArg );
        }


        internal List<Bunch> Build ( )
        {
            var bunches = new List<Bunch> ( );

            for ( var i = 0;    _circuits . Count  >  0;    i++ )
            {
                RenderCircuitOnDictionaries ( _circuits [ i ] );
            }

            var values = _circuitsToBunch . Values;
            var enumerator = values . GetEnumerator ( );
            bunches . Add ( enumerator . Current );

            while ( enumerator . MoveNext ( ) )
            {
                bunches . Add ( enumerator . Current );
            }

            return bunches;
        }


        private void RenderCircuitOnDictionaries ( DependencyCircuit circuit )
        {
            var renderingData = new DataForCircuitRandering ( );

            while ( renderingData . _circuitContinues )
            {
                if (( renderingData._beingProcessedNodeNumber == 0 )    ||    renderingData._metSomeTop    ||    renderingData._isContinuationAfterFork )
                {
                    HandleFirstNode ( renderingData , circuit );
                }
                else
                {
                    HandlePlainNode ( renderingData , circuit );
                }

                renderingData . _beingProcessedNodeNumber++;
            }
        }


        private void HandleFirstNode ( DataForCircuitRandering renderingData , DependencyCircuit beingProcessedCircuit )
        {
            var nodeNumber = renderingData . _beingProcessedNodeNumber;
            var node = beingProcessedCircuit . GetNodeByIndex ( nodeNumber );
            
            if ( _nodeToCircuits . ContainsKey ( node ) )
            {
                renderingData . _obsoleteBunch = _nodeToCircuits [ node ];
                renderingData . _relevantBunch = _nodeToCircuits [ node ] . Clone ( );
                renderingData . _relevantBunch . Add ( beingProcessedCircuit );
                _nodeToCircuits [ node ] = renderingData . _relevantBunch;
            }
            else
            {
                renderingData . _relevantBunch = new List<DependencyCircuit> ( ) { beingProcessedCircuit };
                _nodeToCircuits . Add ( node , renderingData . _relevantBunch );
            }

            var previousNodeMetTop = renderingData . _metSomeTop;
            var nodeIsTopOfCircuit = (( nodeNumber == 0 )    ||    previousNodeMetTop);

            if ( nodeIsTopOfCircuit )
            {
                renderingData . _canRegesterBunch = true;
            }

            renderingData . _metSomeTop = false;
            renderingData . _isContinuationAfterFork = false;
            HandleMeetingTopOrBeingFork ( renderingData , beingProcessedCircuit );
        }


        private void HandlePlainNode ( DataForCircuitRandering renderingData , DependencyCircuit beingProcessedCircuit )
        {
            var nodeNumber = renderingData . _beingProcessedNodeNumber;
            var node = beingProcessedCircuit . GetNodeByIndex ( nodeNumber );

            if ( _nodeToCircuits . ContainsKey ( node ) )
            {
                _nodeToCircuits [ node ] = renderingData . _relevantBunch;
            }
            else
            {
                _nodeToCircuits . Add ( node , renderingData . _relevantBunch );
            }

            HandleMeetingTopOrBeingFork ( renderingData , beingProcessedCircuit );
        }


        private void HandleMeetingTopOrBeingFork ( DataForCircuitRandering renderingData , DependencyCircuit beingProcessedCircuit )
        {
            var nodeNumber = renderingData . _beingProcessedNodeNumber;
            var node = beingProcessedCircuit . GetNodeByIndex ( nodeNumber );
            var nextNode = beingProcessedCircuit . GetNodeByIndex ( nodeNumber + 1 );
            var nodeIsNotLast = beingProcessedCircuit . GetLenght ( ) > ( nodeNumber + 1 );

            if ( nodeIsNotLast )
            {
                var nextIsSomeTop = ( _nodeToCircuits [ node ] . Count )   <=   ( _nodeToCircuits [ nextNode ] . Count );

                if ( nextIsSomeTop )
                {
                    renderingData . _metSomeTop = true;
                }

                var nodeIsFork = ( node . _isFork )    ||    DetectNewFork ( renderingData , beingProcessedCircuit );

                if ( nodeIsFork )
                {
                    RegisterBunch ( renderingData , beingProcessedCircuit );
                }
            }
            else
            {
                renderingData . _circuitContinues = false;
            }
        }


        private bool DetectNewFork ( DataForCircuitRandering renderingData , DependencyCircuit beingProcessedCircuit )
        {
            var nodeNumber = renderingData . _beingProcessedNodeNumber;
            var node = beingProcessedCircuit . GetNodeByIndex ( nodeNumber );
            var nextNode = beingProcessedCircuit . GetNodeByIndex ( nodeNumber + 1 );
            var forkIsDetected = false;

            // 'node' must be contained in more than one circuits for existance of a fork

            var severalCircuitsContainNode = ( _nodeToCircuits [ node ] . Count ) > 1;

            // and node must have true 'isFork' field or
            // level of stack of nodes must decrease to zero in 'next node' for existing a fork in 'node'

            var levelDecreases = ! _nodeToCircuits . ContainsKey ( nextNode );
            forkIsDetected = severalCircuitsContainNode    &&    levelDecreases;
            return forkIsDetected;
        }


        private void RegisterBunch ( DataForCircuitRandering renderingData , DependencyCircuit beingProcessedCircuit )
        {
            var nodeNumber = renderingData . _beingProcessedNodeNumber;
            var node = beingProcessedCircuit . GetNodeByIndex ( nodeNumber );
            var renderedCircuits = _nodeToCircuits [ node ];
            node . _isFork = true;

            if ( renderingData . _canRegesterBunch )
            {
                var nodeRefersToBunch = _circuitsToBunch . ContainsKey ( renderingData . _obsoleteBunch );

                if ( nodeRefersToBunch )
                {
                    var bunch = new Bunch ( renderedCircuits );
                    _circuitsToBunch . Add ( renderedCircuits , bunch );
                    _circuitsToBunch . Remove ( renderingData . _obsoleteBunch );
                }
                else
                {
                    var circuits = new List<DependencyCircuit> ( );
                    circuits . AddRange ( renderedCircuits );
                    _circuitsToBunch . Add ( renderedCircuits , new Bunch ( circuits ) );
                }

                renderingData . _canRegesterBunch = false;
            }

            renderingData . _isContinuationAfterFork = true;
        }

    }
}
