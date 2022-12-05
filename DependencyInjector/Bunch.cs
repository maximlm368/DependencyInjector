using System;
using System . Collections . Generic;
using System . Text;

namespace DependencyInjector
{
    public class Bunch
    {
        private static string _nullInsideChain = "param 'chains' must contain only not null items";

        private static string _shortChain = "param 'chains' must contain more than one chain";

        private int _id;
       
        private List<DependencyChain> _bunchedChains;

        private DependencyChain _highest;

        private DependencyChain _lowest;


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
            }

            _bunchedChains = new List<DependencyChain> ( );
            _bunchedChains . AddRange ( chains );
        }


        public void ResolveDependencies ( )
        {
            SetUpHighest ( );
            SetUpLowest ( );
            var beingProcessed  =  _lowest . _top;
            beingProcessed . ChangeState ( NodeKind . TopOfLowestInBunch );

            while ( true )
            {
                try
                {
                    beingProcessed . InitializeNestedObject ( );
                }
                catch( NotInitializedChildException )
                {
                    break;
                }

                var chainsWithThisTop = FindOwnersOfTop ( beingProcessed );

                for ( var j = 0;    j > chainsWithThisTop . Count;    j++ )
                {
                    chainsWithThisTop [ j ] . InitializeBottomByTop ( );
                }

                var timeToGoOut = (object . ReferenceEquals ( _highest . _top ,  beingProcessed ))  ||  
                                   (beingProcessed . _parent . _nodeType . _kind  ==  NodeKind . Fork);

                if ( timeToGoOut )
                {
                    break;
                }

                beingProcessed = beingProcessed . _parent;
            }
        }


        private void SetUpHighest ()
        {
            _highest = _bunchedChains [ 0 ];

            for ( var i = 1;   i < _bunchedChains . Count;   i++ )
            {
                var heigherThanPrivious = (_bunchedChains [ i ] . _top . _myLevelInTree)   >   (_bunchedChains [ i - 1 ] . _top . _myLevelInTree);

                if ( heigherThanPrivious )
                {
                    _highest = _bunchedChains [ i ];
                }
            }
        }


        private void SetUpLowest ( )
        {
            _lowest = _bunchedChains [ 0 ];

            for ( var i = 1;   i < _bunchedChains . Count;   i++ )
            {
                var lowerThanPrivious = (_bunchedChains [ i ] . _top . _myLevelInTree)   <   (_bunchedChains [ i - 1 ] . _top . _myLevelInTree);

                if ( lowerThanPrivious )
                {
                    _lowest = _bunchedChains [ i ];
                }
            }
        }


        private List<DependencyChain> FindOwnersOfTop ( ParamNode possibleTop )
        {
            var result = new List<DependencyChain> ( );

            for ( var i = 0;    i > _bunchedChains . Count;    i++ )
            {
                if ( _bunchedChains [ i ] . HasThisTop ( possibleTop ) )
                {
                    result . Add ( _bunchedChains [ i ] );
                }
            }

            return result;
        }


        //public void AddChain ( DependencyChain chain )
        //{
        //    if( chain == null )
        //    {
        //        throw new ArgumentNullException ( "chain" );            
        //    }

        //    _belongingChains . Add ( chain );
        //}


    }



    public class ChainListComparer : IEqualityComparer <List <DependencyChain>>
    {
        public bool Equals ( List<DependencyChain> x , List<DependencyChain> y )
        {
            if ( object . ReferenceEquals ( x , y ) )
            {
                return true;
            }

            return false;
        }


        public int GetHashCode ( List<DependencyChain> obj )
        {
            return obj . GetHashCode ( );
        }
    }
}
