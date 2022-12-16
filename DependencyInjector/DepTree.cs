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

        private List<LinkedBunches> _linkedBunches { get; set; }

        private int _deepestLevel { get; set; }

        private Dictionary<ParamNode , List<DependencyChain>> _nodeToChainStack { get; set; }

        private Dictionary<List<DependencyChain> , Bunch> _chainStackToBunch { get; set; }


        public DependencyTree ( Type rootType )
        {
            _root = new ParamNode ( rootType , new OrdinaryNode ( ) );
            _nodes = new List<ParamNode> ( );
            _nodes . Add ( _root );
            _bunches = new List<Bunch> ( );
            var nodeToChainStack = new Dictionary<ParamNode , List<DependencyChain>> ( );
            var chainStackToBunch = new Dictionary<List<DependencyChain> , Bunch> ( new ChainListComparer ( ) );
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
            for ( var i = 0;   i < _chains . Count;   i++ )
            {
                _chains [ i ] . RenderOnMap ( _nodeToChainStack, _chainStackToBunch );
            }

            var values = _chainStackToBunch . Values;
            var enumerator = values . GetEnumerator ( );
            _bunches . Add ( enumerator . Current );

            while ( enumerator . MoveNext ( ) )
            {
                _bunches . Add ( enumerator . Current );
            }
        }


        private void SetLinkedBunches ( )
        {
            for ( var i = 0;    i < _bunches . Count - 1;    i++ )
            {
                if ( ! _bunches [ i ] . _linked )
                {
                    var linked = new List<Bunch> ( );
                    var chains = _bunches [ i ] . GetBunchedChains ( );
                    var chainIds = chains . GetListOfItemPiece ( ( chain ) => { return chain . _id; } );
                    var mask = chainIds . PrintExistenceMask ( );
                    SearchLinkedBunches ( mask , i+1 , linked );

                    if ( linked . Count > 0 )
                    {
                        _bunches [ i ] . _linked = true;
                        linked . Add ( _bunches [ i ] );
                        _linkedBunches . Add ( new LinkedBunches( linked ) );
                    }
                }


            }
        }


        private void SearchLinkedBunches ( IList<bool> mask,  int scratch,  List<Bunch> linked )
        {
            for ( var j = scratch;    j < _bunches . Count - 1;    j++ )
            {
                if ( ! _bunches [ j ] . _linked )
                {
                    var chains = _bunches [ j ] . GetBunchedChains ( );
                    var chainIds = chains . GetListOfItemPiece ( ( chain ) => { return chain . _id; } );
                    var intersection = chainIds . GetIntersectionWhithMask ( mask );

                    if ( intersection . Count > 0 )
                    {
                        _bunches [ j ] . _linked = true;
                        linked . Add ( _bunches [ j ] );
                        var currentMask = chainIds . PrintExistenceMask ( );
                        SearchLinkedBunches ( currentMask , j+1 , linked );
                    }
                }


            }
        }


        private void InitializeNodes ( )
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

        
        private void ArrangeRelations ( )
        {
            var leaves = new List <IGenderTreeNode> ( );

            for ( var i = 0;   i < _chains . Count;   i++ )
            {
                IGenderTreeNode possibleDescendant = null;
                var beingProcessedNode  =  _chains [ i ] . _top;
                SetUpPossibleRelationMember ( _chains [ i ] , possibleDescendant , beingProcessedNode );

                if( possibleDescendant . _renderedOnRelation )
                {
                    continue;
                }

                leaves . Add ( possibleDescendant );
                ConductRelationUntillRelativeIsRenderedOrAbsents ( leaves , possibleDescendant , beingProcessedNode );
            }
        }


        private void SetUpPossibleRelationMember ( DependencyChain chainPresentsPossibleRelative , IGenderTreeNode possibleRelative , ParamNode entryInFirstParam )
        {
            if ( chainPresentsPossibleRelative . _isBunched )
            {
                possibleRelative = chainPresentsPossibleRelative . GetBunchYouAreBunchedIn ( entryInFirstParam , _nodeToChainStack , _chainStackToBunch );
                entryInFirstParam = ( ( Bunch ) possibleRelative ) . _highest . _top;
            }
            else
            {
                possibleRelative = chainPresentsPossibleRelative;
                entryInFirstParam = chainPresentsPossibleRelative . _top;
            }
        }


        private void ConductRelationUntillRelativeIsRenderedOrAbsents ( List<IGenderTreeNode> leaves, IGenderTreeNode possibleDescendant, ParamNode nodeBetweenRelatives )
        {
            while ( true )
            {
                if ( leaves . Contains ( possibleDescendant ) )
                {
                    leaves . Remove ( possibleDescendant );
                    break;
                }

                if ( possibleDescendant . _renderedOnRelation )
                {
                    break;
                }

                nodeBetweenRelatives = nodeBetweenRelatives . _parent;
                var rootIsAchieved = ( nodeBetweenRelatives == null );

                if ( rootIsAchieved )
                {
                    possibleDescendant . _renderedOnRelation = true;
                    break;
                }

                SetUpAncestorIfItFound ( nodeBetweenRelatives , possibleDescendant );
            }
        }


        private void SetUpAncestorIfItFound ( ParamNode nodeBetweenRelatives ,  IGenderTreeNode possibleDescendant )
        {
            IGenderTreeNode ancestor = null;

            if ( _nodeToChainStack . ContainsKey ( nodeBetweenRelatives ) )
            {
                var nodeInAncestor = nodeBetweenRelatives;
                var presentingAncestorChain = _nodeToChainStack [ nodeInAncestor ] [ 0 ];
                var accomplishedDescendant = possibleDescendant;
                SetUpPossibleRelationMember ( presentingAncestorChain , ancestor , nodeInAncestor );
                ancestor . AddChild ( accomplishedDescendant );
                accomplishedDescendant . SetParent ( ancestor );
                accomplishedDescendant . _renderedOnRelation = true;
                possibleDescendant = ancestor;
            }
            else
            {
                possibleDescendant . AddToWayToParent ( nodeBetweenRelatives );
            }
        }


    }
}