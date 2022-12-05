using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;

namespace DependencyInjector
{
    public class DependencyTree
    {
        private DependencyInjection _reflectionTools;

        private ParamNode _root { get; set; }

        private List<ParamNode> _nodes { get; set; }

        private List<DependencyChain> _chains { get; set; }

        private List<Bunch> _bunches { get; set; }

        private int _deepestLevel { get; set; }


        public DependencyTree ( Type rootType )
        {
            _root = new ParamNode ( rootType , new OrdinaryNode ( ) );
            _nodes = new List<ParamNode> ( );
            _nodes . Add ( _root );
            _bunches = new List<Bunch> ( );
        }


        public object BuildRootObject ( )
        {
            BuildYourself ( );
            SetBunches ( );
            InitializeNodes ( );

            if ( _root . _nestedObj . _isInitiolized )
            {
                return _root;
            }

            for ( var i = 0;   i < _bunches . Count;   i++ )
            {
                _bunches [ i ] . ResolveDependencies ( );
            }

            for ( var i = 0;   i < _chains . Count;   i++ )
            {
                for ( var j = 0;   j < _chains . Count;   j++ )
                {
                    _chains [ j ] . ResolveDependency ( );
                }
            }



            if ( _root != null )
                return _root;
            else
            {
                throw new Exception ( " );
            }
        }


        private void BuildYourself ( )
        {
            var currentLevel = new List<ParamNode> ( );
            currentLevel . Add ( _root );
            int levelCounter = 0;

            while ( true )
            {
                var childLevel = new List<ParamNode> ( );
                levelCounter++;

                for ( var i = 0;   i < currentLevel . Count;   i++ )
                {
                    var children = currentLevel [ i ] . DefineChildren ( );
                    GatherExistingChains ( children );
                    childLevel . AddRange ( children );
                    _nodes . AddRange ( children );
                }

                if ( childLevel . Count < 1 )
                {
                    break;
                }

                currentLevel = childLevel;
                _deepestLevel = levelCounter;
            }
        }


        private void GatherExistingChains ( List<ParamNode> nodeChildren )
        {
            for ( var i = 0;   i < nodeChildren . Count;   i++ )
            {
                DependencyChain chain = nodeChildren [ i ] . GetDependencyChainFromAncestors ( );
                _chains . Add ( chain );
            }
        }


        private void SetBunches ( )
        {
            var nodeToChainList = new Dictionary <ParamNode , List <DependencyChain>> ( );
            var chainListToBunch = new Dictionary <List<DependencyChain>, Bunch> ( new ChainListComparer( ) );

            for ( var i = 0;   i < _chains . Count;   i++ )
            {
                _chains [ i ] . RenderOnMap ( nodeToChainList, chainListToBunch );
            }

            var values = chainListToBunch . Values;
            var enumerator = values . GetEnumerator ( );
            _bunches . Add ( enumerator . Current );

            while ( enumerator . MoveNext ( ) )
            {
                _bunches . Add ( enumerator . Current );
            }
        }


        private void InitializeNodes ()
        {
            for ( var i = _nodes . Count - 1;    i > 0;    i-- )
            {
                try
                {
                    _nodes [ i ] . InitializeNestedObject ( );
                }
                catch ( NotInitializedChildException )
                {
                    continue;
                }
            }
        }


        //private void MapChains (  )
        //{
        //    for ( var i = 0;   i < _dependencyChains . Count;   i++ )
        //    {
        //        nodeMap . Add ( _dependencyChains [ i ] , new Bunch ( ) );
        //    }
        //}


        //private bool FindWrapper ( Assembly assemblyForSearching , DependencyChain chain )
        //{
        //    var types = assemblyForSearching . GetExportedTypes ( );

        //    for ( var i = 0;   i < types . Length;   i++ )
        //    {

        //    }

        //    return false;
        //}


        //private void FindChainIntersections ( )
        //{
        //    var forks = new List<ParamNode> ( );

        //    var arr = new List<ParamNode> [ _deepestLevel ];

        //    for ( var i = 0;   i < _dependencyChains . Count;   i++ )
        //    {
        //        this . _dependencyChains [ i ] . Render ( arr );
        //    }

        //    for ( var i = 0;   i < arr . Length;   i++ )
        //    {
        //        forks . AddRange ( arr [ i ] . FindRepeated ( ) );
        //    }

        //    for ( var i = 0; i < forks . Count; i++ )
        //    {
        //        forks [ i ] . SetFork ( );
        //    }
        //}

    }
}