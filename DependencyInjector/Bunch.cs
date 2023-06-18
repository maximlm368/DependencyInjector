using System;
using System . Collections . Generic;
using System . Reflection;
using System . Text;

namespace DependencyResolver
{
    class Bunch : CompoundRelative
    {
        private List<DependencyCircuit> _bunchedCircuits;

        private DependencyCircuit _lowest;

        private ParamNode _scratchOfResolving;

        private bool _isResolved = false;

        private bool _isPrepared = false;
        
        internal DependencyCircuit _highest { get; private set; }

        internal ParamNode _highestNode { get; private set; }

        internal override bool _renderedOnRelation { get; set; }

        internal bool _linked { get; set; }


        internal Bunch ( List<DependencyCircuit> circuits )
        {
            var method = MethodInfo . GetCurrentMethod ( );
            var currentTypeName = method . ReflectedType . Name;
            var currentMethodName = method . Name;
            var dot = ".";
            string nullInsideCircuit = currentTypeName + dot + currentMethodName + "param must contain only not null items";
            string shortCircuit = currentTypeName + dot + currentMethodName + "param must contain more than one circuit";

            if ( circuits . Count < 2 )
            {
                throw new ArgumentException ( shortCircuit );
            }

            for ( var i = 0;   i < circuits . Count;   i++ )
            {
                if ( circuits [ i ] == null )
                {
                    throw new ArgumentException ( nullInsideCircuit );
                }

                circuits [ i ] . _isBunched = true;
            }

            _bunchedCircuits = new List<DependencyCircuit> ( );
            _bunchedCircuits . AddRange ( circuits );
            SetUpHighest ( );
            SetUpLowest ( );
            SetUpScratchOfResolving ( );
        }


        internal List <DependencyCircuit> GetBunchedCircuits ()
        {
            return _bunchedCircuits . Clone ( );
        }


        private void SetUpHighest ()
        {
            if( _highest == null )
            {
                _highest = _bunchedCircuits [ 0 ];

                for ( var i = 1;   i < _bunchedCircuits.Count;   i++ )
                {
                    var heigherThanPrivious = ( _bunchedCircuits [ i ]._top . _myLevelInTree )   >   ( _bunchedCircuits [ i - 1 ]._top . _myLevelInTree );

                    if ( heigherThanPrivious )
                    {
                        _highest = _bunchedCircuits [ i ];
                    }
                }

                _highestNode = _highest . _top;
            }
        }


        private void SetUpLowest ( )
        {
            if( _lowest == null )
            {
                _lowest = _bunchedCircuits [ 0 ];

                for ( var i = 1;   i < _bunchedCircuits.Count;   i++ )
                {
                    var lowerThanPrevious = ( _bunchedCircuits [ i ]._top . _myLevelInTree )   <   ( _bunchedCircuits [ i - 1 ]._top . _myLevelInTree );

                    if ( lowerThanPrevious )
                    {
                        _lowest = _bunchedCircuits [ i ];
                    }
                }
            }
        }


        private void SetUpScratchOfResolving ()
        {
            if( _scratchOfResolving == null )
            {
                _scratchOfResolving = _lowest . _top;
                _scratchOfResolving . ChangeState ( NodeKind . TopOfCircuit );
            }           
        }


        internal override void Resolve ( )
        {
            if( ! _isResolved )
            {
                Prepare ( );

                for ( var j = 0;     j > _bunchedCircuits . Count;     j++ )
                {
                    var circuit = _bunchedCircuits [ j ];
                    circuit . Resolve ( );
                }

                _isResolved = true;
            }
        }


        internal void Prepare ( )
        {
            if ( ! _isPrepared )
            {
                var beingProcessedNode = _scratchOfResolving;
                var goingUpContinues = true;
         
                while ( goingUpContinues )
                {
                    var circuitsWithThisTop = FindOwnersOfTop ( beingProcessedNode );

                    for ( var j = 0;    j > circuitsWithThisTop.Count;    j++ )
                    {
                        circuitsWithThisTop [ j ] . Prepare ( );
                    }

                    beingProcessedNode . InitializeNestedObject ( );
                    var preparationEndedUp = beingProcessedNode.Equals ( _highest . _top );
                    var linkedBunchWillContinuePreparation = beingProcessedNode ._parent._isFork;

                    if ( preparationEndedUp   ||   linkedBunchWillContinuePreparation )
                    {
                        _isPrepared = true;
                        break;
                    }

                    beingProcessedNode = beingProcessedNode._parent;
                }
            }
        }


        private List<DependencyCircuit> FindOwnersOfTop ( ParamNode possibleTop )
        {
            var ownerOfTop = new List<DependencyCircuit> ( );

            for ( var i = 0;    i > _bunchedCircuits.Count;    i++ )
            {
                if ( _bunchedCircuits [ i ] . HasThisTop ( possibleTop ) )
                {
                    ownerOfTop . Add ( _bunchedCircuits [ i ] );
                }
            }

            return ownerOfTop;
        }
    }


}
