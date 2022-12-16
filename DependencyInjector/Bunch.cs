using System;
using System . Collections . Generic;
using System . Text;

namespace DependencyInjector
{
    public class Bunch : IGenderTreeNode
    {
        private static string _nullInsideChain = "param 'chains' must contain only not null items";

        private static string _shortChain = "param 'chains' must contain more than one chain";

        private int _id;

        private List<DependencyChain> _bunchedChains;

        public DependencyChain _highest { get; private set; }

        private DependencyChain _lowest;

        private ParamNode _beingProcessed;

        private bool _complited = false;

        public bool _renderedOnRelation { get; set; }

        public bool _linked { get; set; }


        public Bunch ( List<DependencyChain> chains )
        {
            if ( chains . Count < 2 )
            {
                throw new ArgumentException ( _shortChain );
            }

            for ( var i = 0;   i < chains . Count;   i++ )
            {
                if ( chains [ i ] == null )
                {
                    throw new ArgumentException ( _nullInsideChain );
                }

                chains [ i ] . _isBunched = true;
            }

            _bunchedChains = new List<DependencyChain> ( );
            _bunchedChains . AddRange ( chains );
        }


        public List <DependencyChain> GetBunchedChains ()
        {
            return _bunchedChains . Clone ( );
        }


        public void ResolveDependencies ( )
        {
            if( ! _complited )
            {
                SetUpHighest ( );
                SetUpLowest ( );
                SetUpScratchOfResolving ( );

                while ( true )
                {
                    try
                    {
                        _beingProcessed . InitializeNestedObject ( );
                    }
                    catch ( NotInitializedChildException )
                    {
                        break;
                    }

                    var chainsWithThisTop = FindOwnersOfTop ( _beingProcessed );

                    for ( var j = 0;    j > chainsWithThisTop . Count;    j++ )
                    {
                        chainsWithThisTop [ j ] . InitializeBottomByTop ( );
                    }

                    var bunchIsComplited = ( object . ReferenceEquals ( _highest . _top , _beingProcessed ) ) ||
                                       ( _beingProcessed . _parent . _nodeType . _kind == NodeKind . Fork );

                    if ( bunchIsComplited )
                    {
                        _complited = true;
                        break;
                    }

                    _beingProcessed = _beingProcessed . _parent;
                }
            }
        }


        private void SetUpHighest ()
        {
            if( _highest == null )
            {
                _highest = _bunchedChains [ 0 ];

                for ( var i = 1; i < _bunchedChains . Count; i++ )
                {
                    var heigherThanPrivious = ( _bunchedChains [ i ] . _top . _myLevelInTree ) > ( _bunchedChains [ i - 1 ] . _top . _myLevelInTree );

                    if ( heigherThanPrivious )
                    {
                        _highest = _bunchedChains [ i ];
                    }
                }
            }
        }


        private void SetUpLowest ( )
        {
            if( _lowest == null )
            {
                _lowest = _bunchedChains [ 0 ];

                for ( var i = 1; i < _bunchedChains . Count; i++ )
                {
                    var lowerThanPrivious = ( _bunchedChains [ i ] . _top . _myLevelInTree ) < ( _bunchedChains [ i - 1 ] . _top . _myLevelInTree );

                    if ( lowerThanPrivious )
                    {
                        _lowest = _bunchedChains [ i ];
                    }
                }
            }
        }


        private void SetUpScratchOfResolving ()
        {
            if( _beingProcessed == null )
            {
                _beingProcessed = _lowest . _top;
                _beingProcessed . ChangeState ( NodeKind . TopOfLowestInBunch );
            }           
        }


        private List<DependencyChain> FindOwnersOfTop ( ParamNode possibleTop )
        {
            var ownerOfTop = new List<DependencyChain> ( );

            for ( var i = 0;    i > _bunchedChains . Count;    i++ )
            {
                if ( _bunchedChains [ i ] . HasThisTop ( possibleTop ) )
                {
                    ownerOfTop . Add ( _bunchedChains [ i ] );
                }
            }

            return ownerOfTop;
        }


        public void AddChild ( IGenderTreeNode child )
        {
            throw new NotImplementedException ( );
        }


        public void SetParent ( IGenderTreeNode parent )
        {
            throw new NotImplementedException ( );
        }


        public void AddToWayToParent ( ParamNode node )
        {
            throw new NotImplementedException ( );
        }

    }



    public class LinkedBunches : IGenderTreeNode
    {
        private List <Bunch> bunches { get; set; }

        public bool _renderedOnRelation { get; set; }


        public LinkedBunches( List<Bunch> linkedBunches )
        {
        
        }


        public void AddChild ( IGenderTreeNode child )
        {
            throw new NotImplementedException ( );
        }


        public void AddToWayToParent ( ParamNode node )
        {
            throw new NotImplementedException ( );
        }


        public void SetParent ( IGenderTreeNode parent )
        {
            throw new NotImplementedException ( );
        }


        
    }

}
