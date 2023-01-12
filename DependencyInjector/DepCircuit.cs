using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;

namespace DependencyInjector
{
    class DependencyCircuit : CompoundRelative, IComparable
    {
        private static int _idCounter = 0;

        private bool _isComplited;

        private bool _isPrepared;

        private ParamNode _beingProcessed;

        private List<ParamNode> _nodeChain;

        public int _id { get; private set; }

        public ParamNode _top { get; private set; }

        public ParamNode _bottomCoincidesTop { get; private set; }

        public override bool _renderedOnRelation { get; set; }

        public bool _isBunched { get; set; }
        

        public DependencyCircuit ( List<ParamNode> nodes )
        {
            if ( nodes . Count < 2 )
            {
                throw new ArgumentException ( " );
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
            for ( var i = 0;    i < nodes . Count - 1;    i++ )
            {
                bool parentIsCorrect = Object . ReferenceEquals ( nodes [ i ] . _parent , nodes [ i + 1 ] );

                if ( ( nodes [ i ] == null )     ||     ! parentIsCorrect )
                {
                    throw new Exception ( " );
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


        int IComparable.CompareTo ( object obj )
        {
            var result = 0;

            DependencyCircuit comparable;

            if ( !( obj is DependencyCircuit ) )
            {
                throw new Exception ( ");
            }
            else
            {
                comparable = ( DependencyCircuit ) obj;
            }

            result = _top . _myLevelInTree - comparable . _top . _myLevelInTree;

            return result;
        }


        public bool HasThisTop ( ParamNode possibleTop )
        {
            if( object . ReferenceEquals ( _top , possibleTop ) )
            {
                return true;
            }

            return false;
        }


        private void InitializeBottomByTop ()
        {
            throw new NotImplementedException ( "InitializeBottomByTop" );
        }


        public ParamNode GetNearestOrdinaryAncestor ()
        {
            return _top . GetNearestOrdinaryAncestor ( );
        }


        public ParamNode GetNodeByIndex ( int index )
        {
            return _nodeChain [ index ];
        }


        public int GetLenght ()
        {
            return _nodeChain . Count;
        }


        public int GetNodeIndex ( ParamNode beingProcessed )
        {
            var result = _nodeChain . IndexOf ( beingProcessed );

            if ( result >= 0 )
            {
                return _nodeChain . IndexOf ( beingProcessed );
            }

            throw new Exception ( );
        }


        public override void Resolve ( )
        {
            if( ! _isComplited )
            {
                Prepare ( );
                var goingUpContinues = true;

                while ( goingUpContinues )
                {
                    //in this case we skip attempt if child is not initialized so we will continue attempt again 
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
                        _isComplited = true;
                        break;
                    }

                    _beingProcessed  =  _beingProcessed . _parent;
                }
            }
        }


        public void Prepare ( )
        {
            if ( ! _isPrepared )
            {
                 //we try to initialize top of circuit and don't catch exceptions because if something is wrong ( it's not supposed to happen )
                 //we should not process it and throw to enter of layer

                _top . InitializeNestedObject ( );
                InitializeBottomByTop ( );
                _bottomCoincidesTop . InitializeNestedObject ( _top );
                _beingProcessed = _bottomCoincidesTop . _parent;
                _isPrepared = true;
            }
        }

    }

}