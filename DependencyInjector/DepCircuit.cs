using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;

namespace DependencyResolver
{
    class DependencyCircuit : CompoundRelative
    {
        private static int _idCounter = 0;

        private bool _isResolved;

        private bool _isPrepared;

        private ParamNode _beingProcessed;

        private List<ParamNode> _nodeChain;

        internal int _id { get; private set; }

        internal ParamNode _top { get; private set; }

        internal ParamNode _bottomCoincidesTop { get; private set; }

        internal override bool _renderedOnRelation { get; set; }

        internal bool _isBunched { get; set; }
        

        internal DependencyCircuit ( List<ParamNode> nodes )
        {
            var argIsIncorrect = "arg 'nodes' as list must have more then two items";

            if ( nodes . Count < 2 )
            {
                throw new ArgumentException ( argIsIncorrect );
            }

            VerifyChainConsistency ( nodes );
            VerifyTopBottomTypeCoincidence ( nodes );
            _nodeChain = nodes . Clone ( );
            _bottomCoincidesTop = _nodeChain . First ( );
            _top = _nodeChain . Last ( );
            _isBunched = false;
            _id = _idCounter;
            _idCounter++;
        }


        private void VerifyChainConsistency ( List<ParamNode> nodes )
        {
            var nullItem = "arg 'nodes' cant contain null item";
            var incorrectParent = "next to item doesnt coincide with its parent and number of item is ";

            for ( var i = 0;    i < nodes . Count - 1;    i++ )
            {
                if ( nodes [ i ] == null )
                {
                    throw new ArgumentNullException ( nullItem );
                }

                bool parentIsNotCorrect = ! Object . ReferenceEquals ( nodes [ i ] . _parent , nodes [ i + 1 ] );

                if ( parentIsNotCorrect )
                {
                    throw new Exception ( incorrectParent + i );
                }
            }
        }


        private void VerifyTopBottomTypeCoincidence ( List<ParamNode> nodes )
        {   
            string topDoesNotCoincideBottom = "first item in list of nodes must coincide last";
            
            if ( ! nodes . First ( ) . TypeNameEquals ( nodes . Last ( ) ) )
            {
                throw new Exception ( topDoesNotCoincideBottom );
            }
        }


        internal bool HasThisTop ( ParamNode possibleTop )
        {
            if( object . ReferenceEquals ( _top , possibleTop ) )
            {
                return true;
            }

            return false;
        }


        internal ParamNode GetNearestOrdinaryAncestor ()
        {
            return _top . GetNearestOrdinaryAncestor ( );
        }


        internal ParamNode GetNodeByIndex ( int index )
        {
            return _nodeChain [ index ];
        }


        internal int GetLenght ()
        {
            return _nodeChain . Count;
        }


        internal int GetNodeIndex ( ParamNode beingProcessed )
        {
            var result = _nodeChain . IndexOf ( beingProcessed );

            if ( result >= 0 )
            {
                return _nodeChain . IndexOf ( beingProcessed );
            }

            throw new Exception ( );
        }


        internal override void Resolve ( )
        {
            if( ! _isResolved )
            {
                Prepare ( );
                var goingUpContinues = true;

                while ( goingUpContinues )
                {
                    //in case the circuit is bunched
                    //we skip attempt if child is not initialized so we will continue attempt again 
                    //for example if we process fork node in bunch and intersecting circuit is not ready
                    //then we will resolve intersecting circuit we use result (not complited) of this circuit

                    try
                    {
                        _beingProcessed . InitializeNestedObject ( );
                    }
                    catch ( NotInitializedChildException )
                    {
                        break;
                    }

                    var arrivedToTop = _top . Equals ( _beingProcessed . _parent );

                    if ( arrivedToTop )
                    {
                        _top . EndUpIinitialization ( );
                        _isResolved = true;
                        break;
                    }

                    _beingProcessed  =  _beingProcessed . _parent;
                }
            }
        }


        internal void Prepare ( )
        {
            if ( ! _isPrepared )
            {
                 //we try to initialize top of circuit and don't catch exceptions because if something is wrong ( it's not supposed to happen )
                 //we should not process it and throw to enter of layer

                _top . InitializeNestedObject ( );
                _bottomCoincidesTop . InitializeNestedObject ( _top );
                _beingProcessed = _bottomCoincidesTop . _parent;
                _isPrepared = true;
            }
        }

    }

}