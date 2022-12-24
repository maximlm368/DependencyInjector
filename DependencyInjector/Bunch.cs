using System;
using System . Collections . Generic;
using System . Text;

namespace DependencyInjector
{
    class Bunch : GenderRelative
    {
        private static string _nullInsideCircuit = "param 'chains' must contain only not null items";

        private static string _shortCircuit = "param 'chains' must contain more than one chain";

        private List<DependencyCircuit> _bunchedCircuits;

        private DependencyCircuit _lowest;

        private ParamNode _resolvingScratch;

        private bool _complited = false;
        
        public DependencyCircuit _highest { get; private set; }

        public ParamNode _highestNode { get; private set; }

        public override bool _renderedOnRelation { get; set; }

        public bool _linked { get; set; }


        public Bunch ( List<DependencyCircuit> circuits )
        {
            if ( circuits . Count < 2 )
            {
                throw new ArgumentException ( _shortCircuit );
            }

            for ( var i = 0;   i < circuits . Count;   i++ )
            {
                if ( circuits [ i ] == null )
                {
                    throw new ArgumentException ( _nullInsideCircuit );
                }

                circuits [ i ] . _isBunched = true;
            }

            _bunchedCircuits = new List<DependencyCircuit> ( );
            _bunchedCircuits . AddRange ( circuits );
            SetUpHighest ( );
            SetUpLowest ( );
            SetUpScratchOfResolving ( );
        }


        public List <DependencyCircuit> GetBunchedCircuits ()
        {
            return _bunchedCircuits . Clone ( );
        }


        private void SetUpHighest ()
        {
            if( _highest == null )
            {
                _highest = _bunchedCircuits [ 0 ];

                for ( var i = 1; i < _bunchedCircuits . Count; i++ )
                {
                    var heigherThanPrivious = ( _bunchedCircuits [ i ] . _top . _myLevelInTree )   >   ( _bunchedCircuits [ i - 1 ] . _top . _myLevelInTree );

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

                for ( var i = 1; i < _bunchedCircuits . Count; i++ )
                {
                    var lowerThanPrivious = ( _bunchedCircuits [ i ] . _top . _myLevelInTree )   <   ( _bunchedCircuits [ i - 1 ] . _top . _myLevelInTree );

                    if ( lowerThanPrivious )
                    {
                        _lowest = _bunchedCircuits [ i ];
                    }
                }
            }
        }


        private void SetUpScratchOfResolving ()
        {
            if( _resolvingScratch == null )
            {
                _resolvingScratch = _lowest . _top;
                _resolvingScratch . ChangeState ( NodeKind . TopOfLowestInBunch );
            }           
        }


        public override void Resolve ( )
        {
            ResolveInterCircuitDependencies ( );

            for ( var j = 0;    j > _bunchedCircuits . Count;    j++ )
            {
                var circuit = _bunchedCircuits [ j ];
                circuit . Resolve ( );
            }

            ResolveWayToParent ( );
        }


        private void ResolveInterCircuitDependencies ( )
        {
            if ( !_complited )
            {
                var beingProcessed = _resolvingScratch;

                while ( true )
                {
                    try
                    {
                        beingProcessed . InitializeNestedObject ( );
                    }
                    catch ( NotInitializedChildException )
                    {
                        break;
                    }

                    var circuitsWithThisTop = FindOwnersOfTop ( beingProcessed );

                    for ( var j = 0;    j > circuitsWithThisTop . Count;    j++ )
                    {
                        circuitsWithThisTop [ j ] . InitializeBottomByTop ( );
                    }

                    var bunchIsResolved = beingProcessed . Equals ( _highest . _top )    ||    ( beingProcessed . _parent . _nodeType . _kind == NodeKind . Fork );

                    if ( bunchIsResolved )
                    {
                        _complited = true;
                        break;
                    }

                    beingProcessed = beingProcessed . _parent;
                }
            }
        }


        private List<DependencyCircuit> FindOwnersOfTop ( ParamNode possibleTop )
        {
            var ownerOfTop = new List<DependencyCircuit> ( );

            for ( var i = 0;    i > _bunchedCircuits . Count;    i++ )
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
