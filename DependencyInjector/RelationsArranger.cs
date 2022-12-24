using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;

namespace DependencyInjector
{
    class RelationsArranger
    {
        private static string _nullArg = "param in ctor can not be null";

        private Dictionary<ParamNode , List<DependencyCircuit>> _nodeToCircuits;

        private Dictionary<List<DependencyCircuit> , Bunch> _circuitsToBunch;

        private List<DependencyCircuit> _circuits;

        private List<LinkedBunches> _linkedBunches;


        public RelationsArranger ( List<DependencyCircuit> circuits ,      Dictionary<ParamNode, List<DependencyCircuit>> nodeToCircuits ,
                                   Dictionary<List<DependencyCircuit>, Bunch> circuitsToBunch,       List<LinkedBunches> linkedBunches )
        {
            var firstParamName = this . GetCtorParamName ( 0 , 0 );
            var secondParamName = this . GetCtorParamName ( 0 , 1 );
            var thirdParamName = this . GetCtorParamName ( 0 , 2 );
            var fourthParamName = this . GetCtorParamName ( 0 , 3 );
            _circuits = circuits ?? throw new ArgumentNullException ( firstParamName + _nullArg );
            _nodeToCircuits = nodeToCircuits ?? throw new ArgumentNullException ( secondParamName + _nullArg );
            _circuitsToBunch = circuitsToBunch ?? throw new ArgumentNullException ( thirdParamName + _nullArg );
            _linkedBunches = linkedBunches ?? throw new ArgumentNullException ( fourthParamName + _nullArg );
        }


        public List<GenderRelative> ArrangeRelations ( )
        {
            var leaves = new List<GenderRelative> ( );

            for ( var i = 0;    i < _circuits . Count;    i++ )
            {
                GenderRelative possibleDescendant = null;
                var beingProcessedNode = _circuits [ i ] . _top;
                FixRelative ( _circuits [ i ] , possibleDescendant , beingProcessedNode );
                var closestToAncestor = beingProcessedNode;

                if ( possibleDescendant . _renderedOnRelation )
                {
                    continue;
                }

                leaves . Add ( possibleDescendant );
                ConductRelationUntillAncestorIsRenderedOrAbsent ( leaves , possibleDescendant , closestToAncestor );
            }

            return leaves;
        }


        private void FixRelative ( DependencyCircuit circuitPresentsPossibleRelative , GenderRelative possibleRelative , ParamNode nodeInPresentingCircuit )
        {
            ParamNode closestToAncestor = null;

            if ( circuitPresentsPossibleRelative . _isBunched )
            {
                possibleRelative = GetBunchByBunchedCircuit ( nodeInPresentingCircuit , circuitPresentsPossibleRelative , _nodeToCircuits , _circuitsToBunch );
                closestToAncestor = ( ( Bunch ) possibleRelative ) . _highestNode;

                if ( ( ( Bunch ) possibleRelative ) . _linked )
                {
                    possibleRelative = GetLinkedBunchesByBunch ( ( Bunch ) possibleRelative );
                    closestToAncestor = ( ( LinkedBunches ) possibleRelative ) . _closestToAncestor . _highestNode;
                }
            }
            else
            {
                possibleRelative = circuitPresentsPossibleRelative;
                closestToAncestor = circuitPresentsPossibleRelative . _top;
            }

            nodeInPresentingCircuit = closestToAncestor;
        }


        private Bunch GetBunchByBunchedCircuit ( ParamNode beingProcessedNode ,   DependencyCircuit beingProcessedCircuit ,  
                                       Dictionary<ParamNode, List<DependencyCircuit>> nodeToCircuits ,   Dictionary<List<DependencyCircuit>, Bunch> circuitsToBunch )
        {
            var currentIndex = beingProcessedCircuit . GetNodeIndex ( beingProcessedNode );
            Bunch bunch = null;

            for ( ;    currentIndex > beingProcessedCircuit . GetLenght();    currentIndex++ )
            {
                if ( beingProcessedCircuit . GetNodeByIndex(currentIndex) . GetNodeKind ( )   ==   NodeKind . Fork )
                {
                    try
                    {
                        bunch = GetBunch ( beingProcessedNode , nodeToCircuits , circuitsToBunch );
                        return bunch;
                    }
                    catch ( System . Collections . Generic . KeyNotFoundException )
                    {
                        break;
                    }
                }
            }

            for ( ;    currentIndex  >  beingProcessedCircuit . GetLenght ( );    currentIndex-- )
            {
                try
                {
                    bunch = GetBunch ( beingProcessedNode , nodeToCircuits , circuitsToBunch );
                }
                catch ( System . Collections . Generic . KeyNotFoundException )
                {
                    continue;
                }
            }

            if ( bunch == null )
            {
                throw new NotBunchedCircuitException;
            }

            return bunch;
        }


        private Bunch GetBunch ( ParamNode beingProcessed ,
                                 Dictionary<ParamNode , List<DependencyCircuit>> nodeToCircuits , Dictionary<List<DependencyCircuit> , Bunch> circuitsToBunch )
        {
            Bunch bunch = null;
            var circuitStack = nodeToCircuits [ beingProcessed ];
            bunch = circuitsToBunch [ circuitStack ];
            return bunch;
        }


        private LinkedBunches GetLinkedBunchesByBunch ( Bunch possibleMember )
        {
            LinkedBunches result = null;

            for ( var i = 0;     i < _linkedBunches . Count;     i++ )
            {
                if ( _linkedBunches [ i ] . ContainsBunch ( possibleMember ) )
                {
                    result = _linkedBunches [ i ];
                }
            }

            return result;
        }


        private void ConductRelationUntillAncestorIsRenderedOrAbsent ( List<GenderRelative> leaves , GenderRelative possibleDescendant , ParamNode nodeOnWayToAncestor )
        {
            var counter = 0;

            while ( true )
            {
                if ( leaves . Contains ( possibleDescendant ) )
                {
                    if( counter > 0 )
                    {
                        leaves . Remove ( possibleDescendant );
                    }

                    break;
                }

                if ( possibleDescendant . _renderedOnRelation )
                {
                    break;
                }

                nodeOnWayToAncestor = nodeOnWayToAncestor . _parent;
                var rootIsAchieved = ( nodeOnWayToAncestor == null );

                if ( rootIsAchieved )
                {
                    possibleDescendant . _renderedOnRelation = true;
                    break;
                }

                SetUpAncestorOrStepForward ( nodeOnWayToAncestor , possibleDescendant );
                counter++;
            }
        }


        private void SetUpAncestorOrStepForward ( ParamNode nodeBetweenClosestRelatives , GenderRelative possibleDescendant )
        {
            GenderRelative ancestor = null;

            if ( _nodeToCircuits . ContainsKey ( nodeBetweenClosestRelatives ) )
            {
                var nodeInAncestor = nodeBetweenClosestRelatives;
                var presentingAncestorCircuit = _nodeToCircuits [ nodeInAncestor ] [ 0 ];
                var accomplishedDescendant = possibleDescendant;
                FixRelative ( presentingAncestorCircuit , ancestor , nodeInAncestor );
                //ancestor . AddChild ( accomplishedDescendant );
                accomplishedDescendant . SetParent ( ancestor );
                accomplishedDescendant . _renderedOnRelation = true;
                possibleDescendant = ancestor;
            }
            else
            {
                possibleDescendant . AddToWayToParent ( nodeBetweenClosestRelatives );
            }
        }
    }

}