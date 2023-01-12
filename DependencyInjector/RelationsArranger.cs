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


        public List<CompoundRelative> ArrangeRelations ( )
        {
            var descendantLesses = new List<CompoundRelative> ( );

            for ( var i = 0;    i < _circuits . Count;    i++ )
            {
                CompoundRelative possibleDescendantPresentedByCurrentCircuit = null;
                ParamNode closestToAncestorInPossibleDescendant = _circuits [ i ] . _top;
                SetUpRelative ( _circuits [ i ] , possibleDescendantPresentedByCurrentCircuit , closestToAncestorInPossibleDescendant );

                if ( possibleDescendantPresentedByCurrentCircuit . _renderedOnRelation )
                {
                    continue;
                }

                descendantLesses . Add ( possibleDescendantPresentedByCurrentCircuit );
                ConductRelationUntillAncestorIsRenderedOrAbsent ( descendantLesses , possibleDescendantPresentedByCurrentCircuit , closestToAncestorInPossibleDescendant );
            }

            return descendantLesses;
        }


        private void SetUpRelative ( DependencyCircuit circuitPresentsPossibleRelative , CompoundRelative possibleRelative , ParamNode closestToAncestor )
        {
            if ( circuitPresentsPossibleRelative . _isBunched )
            {
                possibleRelative = GetBunchByBunchedCircuit ( closestToAncestor , circuitPresentsPossibleRelative );
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
        }


        private Bunch GetBunchByBunchedCircuit ( ParamNode inBeingProcessedCircuit ,   DependencyCircuit beingProcessedCircuit )
        {
            var currentIndex = beingProcessedCircuit . GetNodeIndex ( inBeingProcessedCircuit );
            Bunch bunch = null;
            
            GoTowardBottom ( currentIndex , bunch , beingProcessedCircuit );

            if ( bunch != null )
            {
                return bunch;
            }

            GoTowardTop ( currentIndex , bunch , beingProcessedCircuit );

            if ( bunch == null )
            {
                throw new NotBunchedCircuitException ( );
            }

            return bunch;
        }


        private void GoTowardTop ( int currentIndex, Bunch bunch, DependencyCircuit beingProcessedCircuit )
        {
            for ( ;    currentIndex >= 0;    currentIndex-- )
            {
                var currentNode = beingProcessedCircuit . GetNodeByIndex ( currentIndex );
                
                if ( currentNode . _isFork )
                {
                    try
                    {
                        bunch = GetBunch ( currentNode );
                        return;
                    }
                    catch ( System . Collections . Generic . KeyNotFoundException )
                    {
                        continue;
                    }
                }
            }
        }


        private void GoTowardBottom ( int currentIndex , Bunch bunch , DependencyCircuit beingProcessedCircuit )
        {
            for ( ;    currentIndex > beingProcessedCircuit . GetLenght ( );    currentIndex++ )
            {
                var currentNode = beingProcessedCircuit . GetNodeByIndex ( currentIndex );

                if ( currentNode . _isFork )
                {
                    try
                    {
                        bunch = GetBunch ( currentNode );
                        return;
                    }
                    catch ( System . Collections . Generic . KeyNotFoundException )
                    {
                        // on highest fork ( in terms of level in tree ) in bunch that bunch is registered 

                        break;
                    }
                }
            }
        }


        private Bunch GetBunch ( ParamNode beingProcessed )
        {
            Bunch bunch = null;
            var circuitsThatHaveNode = _nodeToCircuits [ beingProcessed ];
            bunch = _circuitsToBunch [ circuitsThatHaveNode ];
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


        private void ConductRelationUntillAncestorIsRenderedOrAbsent ( List <CompoundRelative> descendantLesses ,
                                                                       CompoundRelative possibleDescendant , ParamNode closestToAncestor )
        {
            var counter = 0;

            while ( true )
            {
                var descendantLessAncestorIsAchieved = descendantLesses . Contains ( possibleDescendant )     &&     (counter > 0);

                if ( descendantLessAncestorIsAchieved )
                {
                    descendantLesses . Remove ( possibleDescendant );
                    break;
                }

                if ( possibleDescendant . _renderedOnRelation )
                {
                    break;
                }

                closestToAncestor = closestToAncestor . _parent;
                var rootIsAchieved = ( closestToAncestor == null );

                if ( rootIsAchieved )
                {
                    possibleDescendant . _renderedOnRelation = true;
                    break;
                }

                SetUpAncestorOrStepTowardIt ( closestToAncestor , possibleDescendant );
                counter++;
            }
        }


        private void SetUpAncestorOrStepTowardIt ( ParamNode closestToAncestor , CompoundRelative possibleDescendant )
        {
            CompoundRelative ancestor = null;

            if ( _nodeToCircuits . ContainsKey ( closestToAncestor ) )
            {
                var thatAchievedAncestor = closestToAncestor;
                var presentingAncestorCircuit = _nodeToCircuits [ thatAchievedAncestor ] [ 0 ];
                var accomplishedDescendant = possibleDescendant;
                SetUpRelative ( presentingAncestorCircuit , ancestor , thatAchievedAncestor );
                //ancestor . AddChild ( accomplishedDescendant );
                accomplishedDescendant . SetClosestAncestor ( ancestor );
                accomplishedDescendant . _renderedOnRelation = true;
                possibleDescendant = ancestor;
            }
            else
            {
                possibleDescendant . AddToWayToClosestAncestorOrRoot ( closestToAncestor );
            }
        }
    }

}