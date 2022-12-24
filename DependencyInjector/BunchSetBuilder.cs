using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;

namespace DependencyInjector
{
    class BunchSetBuilder
    {
        private class DataForCircuitRandering
        {
            public int _beingProcessedNodeNumber = 0;

            public List<DependencyCircuit> _relevantStack = null;

            public List<DependencyCircuit> _obsoleteStack = null;

            public bool _metSomeTop = false;

            public bool _canRegesterBunch = true;

            public bool _circuitContinues = true;

            public bool _isContinuationAfterFork = false;
        }


        private Dictionary<ParamNode , List<DependencyCircuit>> _nodeToCircuits;

        private Dictionary<List<DependencyCircuit> , Bunch> _circuitsToBunch;

        private List<DependencyCircuit> _circuits;

        private static string _nullArg = "'circuits' param in ctor can not be null";


        public BunchSetBuilder ( List<DependencyCircuit> circuits,   Dictionary <ParamNode, List <DependencyCircuit>> nodeToCircuits,
                                                                                            Dictionary <List <DependencyCircuit>, Bunch> circuitsToBunch )
        {
            _nodeToCircuits = nodeToCircuits ?? throw new ArgumentNullException ( _nullArg );
            _circuitsToBunch = circuitsToBunch ?? throw new ArgumentNullException ( _nullArg );
            _circuits = circuits ?? throw new ArgumentNullException ( _nullArg );
        }


        public List<Bunch> Build ( )
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
                if ( ( renderingData . _beingProcessedNodeNumber == 0 )    ||    renderingData . _metSomeTop    ||    renderingData . _isContinuationAfterFork )
                {
                    HandleFirstNode ( renderingData , circuit );
                }
                else
                {
                    HandlePlainNode ( renderingData , circuit );
                }

                renderingData . _beingProcessedNodeNumber++;

                for ( ;     renderingData . _beingProcessedNodeNumber  <  circuit . GetLenght ( );     renderingData . _beingProcessedNodeNumber++ )
                {

                }
            }
        }


        private void HandleFirstNode ( DataForCircuitRandering renderingData , DependencyCircuit beingProcessedCircuit )
        {
            var nodeNumber = renderingData . _beingProcessedNodeNumber;
            var node = beingProcessedCircuit . GetNodeByIndex ( nodeNumber );
            
            if ( _nodeToCircuits . ContainsKey ( node ) )
            {
                renderingData . _obsoleteStack = _nodeToCircuits [ node ];
                renderingData . _relevantStack = _nodeToCircuits [ node ] . Clone ( );
                renderingData . _relevantStack . Add ( beingProcessedCircuit );
                _nodeToCircuits [ node ] = renderingData . _relevantStack;
            }
            else
            {
                renderingData . _relevantStack = new List<DependencyCircuit> ( ) { beingProcessedCircuit };
                _nodeToCircuits . Add ( node , renderingData . _relevantStack );
            }

            var nodeIsTopOfCircuit = (renderingData . _metSomeTop)    ||    (nodeNumber == 0);

            if ( nodeIsTopOfCircuit )
            {
                renderingData . _canRegesterBunch = true;
            }

            renderingData . _metSomeTop = false;
            renderingData . _isContinuationAfterFork = false;
            Continue ( renderingData , beingProcessedCircuit );
        }


        private void HandlePlainNode ( DataForCircuitRandering renderingData , DependencyCircuit beingProcessedCircuit )
        {
            var nodeNumber = renderingData . _beingProcessedNodeNumber;
            var node = beingProcessedCircuit . GetNodeByIndex ( nodeNumber );

            if ( _nodeToCircuits . ContainsKey ( node ) )
            {
                _nodeToCircuits [ node ] = renderingData . _relevantStack;
            }
            else
            {
                _nodeToCircuits . Add ( node , renderingData . _relevantStack );
            }

            Continue ( renderingData , beingProcessedCircuit );
        }


        private void Continue ( DataForCircuitRandering renderingData , DependencyCircuit beingProcessedCircuit )
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

                var nodeIsFork = ( node . GetNodeKind ( )  ==  NodeKind . Fork )    ||    DetectNewFork ( renderingData , beingProcessedCircuit );

                if ( nodeIsFork )
                {
                    HandleFork ( renderingData , beingProcessedCircuit );
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

            // 'node' must be contained in more than one circuits for existing of a fork

            var severalCircuitsContainNode = ( _nodeToCircuits [ node ] . Count ) > 1;

            // node must have  'NodeKind . Fork' of 'nodeType' or
            // level of stack of nodes must decrease to zero in 'next node' for existing a fork in 'node'

            var levelDecreases = ! _nodeToCircuits . ContainsKey ( nextNode );
            forkIsDetected = severalCircuitsContainNode    &&    levelDecreases;
            return forkIsDetected;
        }


        private void HandleFork ( DataForCircuitRandering renderingData , DependencyCircuit beingProcessedCircuit )
        {
            var nodeNumber = renderingData . _beingProcessedNodeNumber;
            var node = beingProcessedCircuit . GetNodeByIndex ( nodeNumber );
            var renderedCircuits = _nodeToCircuits [ node ];
            node . ChangeState ( NodeKind . Fork );

            if ( renderingData . _canRegesterBunch )
            {
                var nodeRefersToBunch = ( renderingData . _obsoleteStack != null )    &&    ( _circuitsToBunch . ContainsKey ( renderingData . _obsoleteStack ) );

                if ( nodeRefersToBunch )
                {
                    var bunch = new Bunch ( renderedCircuits );
                    _circuitsToBunch . Add ( renderedCircuits , bunch );
                    _circuitsToBunch . Remove ( renderingData . _obsoleteStack );
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
