using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;

namespace DependencyInjector
{
    public class DependencyTree
    {
        private ParamNode _root;

        private List<ParamNode> _nodes;

        private List<DependencyCircuit> _circuits;

        private List<Bunch> _bunches;

        private List<LinkedBunches> _linkedBunches;

        private int _deepestLevel;

        private Dictionary<ParamNode , List<DependencyCircuit>> _nodeToCircuits;

        private Dictionary<List<DependencyCircuit> , Bunch> _circuitsToBunch;


        public DependencyTree ( Type rootType )
        {
            _root = new ParamNode ( rootType , new OrdinaryNode ( ) );
            _nodes = new List<ParamNode> ( );
            _nodes . Add ( _root );
            _bunches = new List<Bunch> ( );
            _nodeToCircuits = new Dictionary<ParamNode , List<DependencyCircuit>> ( );
            _circuitsToBunch = new Dictionary<List<DependencyCircuit> , Bunch> ( new CircuitListComparer ( ) );
        }


        public object BuildRootObject ( )
        {
            BuildYourself ( );
            InitializeNodes ( );

            if ( _root . _nestedObj . _isInitiolized )
            {
                return _root;
            }

            var bunchSetBuilder = new BunchSetBuilder ( _circuits , _nodeToCircuits , _circuitsToBunch );
            _bunches = bunchSetBuilder . Build ( );
            SetLinkedBunches ( );
            var relationArranger = new RelationsArranger ( _circuits , _nodeToCircuits , _circuitsToBunch , _linkedBunches );
            var leaves = relationArranger . ArrangeRelations ( );
            InitializeNodes ( );



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
                    GatherExistingCircuits ( children );
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


        private void GatherExistingCircuits ( List<ParamNode> nodes )
        {
            for ( var i = 0;    i < nodes . Count;    i++ )
            {
                List<ParamNode> nodeSequence = nodes [ i ] . GetSequenceForDependencyCircuit ( );
                DependencyCircuit circuit = CreateCircuit ( nodeSequence );

                if ( circuit != null )
                {
                     _circuits . Add ( circuit );
                }
            }
        }

        
        private DependencyCircuit CreateCircuit ( List <ParamNode> possibleCircuit )
        {
            DependencyCircuit result = null;
            possibleCircuit . Reverse ( );

            if ( possibleCircuit . Count  >  2 )
            {
                possibleCircuit . AccomplishForEach<ParamNode>
                (
                   ( item ) => { if ( item . GetNodeKind ( )  !=  NodeKind . DependencyCycleParticipant ) { item . ChangeState ( NodeKind . DependencyCycleParticipant ); } }
                );

                result = new DependencyCircuit ( possibleCircuit );
            }

            return result;
        }


        private void SetLinkedBunches ( )
        {
            if( _bunches . Count < 1 )
            {
                return;
            }

            for ( var i = 0;    i < _bunches . Count - 1;    i++ )
            {
                var beingProcessed = _bunches [ i ];

                if ( ! beingProcessed . _linked )
                {
                    var linked = new List<Bunch> ( );
                    var concerneds = new List<int> ( );
                    concerneds . Add ( i );
                    var circuits = beingProcessed . GetBunchedCircuits ( );
                    var circuitIds = circuits . GetListOfItemPiece ( ( circuit ) => { return circuit . _id; } );
                    var mask = circuitIds . PrintMaskOfPresence ( );
                    SearchLinkedBunches ( mask , i+1 , linked , concerneds );

                    if ( linked . Count > 0 )
                    {
                        beingProcessed . _linked = true;
                        linked . Add ( beingProcessed );
                        _linkedBunches . Add ( new LinkedBunches( linked ) );
                    }
                }
            }
        }


        private void SearchLinkedBunches ( IList<bool> mask,  int scratch,  List<Bunch> linked,  List<int> concerneds )
        {
            for ( var j = scratch;    j < _bunches . Count - 1;    j++ )
            {
                var mustBeIgnored = concerneds . Contains ( j );

                if ( ! _bunches [ j ] . _linked      &&      ! mustBeIgnored )
                {
                    var circuits = _bunches [ j ] . GetBunchedCircuits ( );
                    var circuitIds = circuits . GetListOfItemPiece ( ( circuit ) => { return circuit . _id; } );
                    var intersection = circuitIds . GetIntersectionWhithMask ( mask );

                    if ( intersection . Count > 0 )
                    {
                        _bunches [ j ] . _linked = true;
                        linked . Add ( _bunches [ j ] );
                        concerneds . Add ( j );
                        var currentMask = circuitIds . PrintMaskOfPresence ( );
                        SearchLinkedBunches ( currentMask , 0 , linked , concerneds );
                    }
                }
            }
        }


        private void InitializeNodes ( )
        {
            for ( var i = _nodes . Count - 1;    i >= 0;    i-- )
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


        private void ResolveRelatives ( List <GenderRelative> leaves )
        {
            var descendants = leaves;

            while ( true )
            {
                var ancestors = new List<GenderRelative> ( );

                for ( var i = 0;    i > descendants . Count;    i++ )
                {
                    descendants [ i ] . Resolve ( );
                    

                    

                    ancestors . Add ( descendants [ i ] . GetParent ( ) );
                }

                if( ancestors . Count < 1 )
                {
                    break;
                }

                descendants = ancestors;
            }


        }

        
        //private void ArrangeRelations ( )
        //{
        //    var leaves = new List <IGenderRelative> ( );

        //    for ( var i = 0;   i < _circuits . Count;   i++ )
        //    {
        //        IGenderRelative possibleDescendant = null;
        //        var beingProcessedNode  =  _circuits [ i ] . _top;
        //        FixRelative ( _circuits [ i ] , possibleDescendant , beingProcessedNode );

        //        if( possibleDescendant . _renderedOnRelation )
        //        {
        //            continue;
        //        }

        //        leaves . Add ( possibleDescendant );
        //        ConductRelationUntillRelativeIsRenderedOrAbsents ( leaves , possibleDescendant , beingProcessedNode );
        //    }
        //}


        //private void FixRelative ( DependencyCircuit circuitPresentsPossibleRelative , IGenderRelative possibleRelative , ParamNode entryInFirstParam )
        //{
        //    if ( circuitPresentsPossibleRelative . _isBunched )
        //    {
        //        possibleRelative = circuitPresentsPossibleRelative . GetBunchYouAreBunchedIn ( entryInFirstParam , _nodeToCircuits , _circuitsToBunch );
        //        entryInFirstParam = ( ( Bunch ) possibleRelative ) . _highest . _top;

        //        if ( ( ( Bunch ) possibleRelative ) . _linked )
        //        {
        //            possibleRelative = GetLinkedBunchesByBunch ( ( Bunch ) possibleRelative );
        //            entryInFirstParam = ( ( LinkedBunches ) possibleRelative ) . _closestToAncestor . _highest . _top;
        //        }
        //    }
        //    else
        //    {
        //        possibleRelative = circuitPresentsPossibleRelative;
        //        entryInFirstParam = circuitPresentsPossibleRelative . _top;
        //    }
        //}


        //private LinkedBunches GetLinkedBunchesByBunch ( Bunch possibleMember )
        //{
        //    LinkedBunches result = null;

        //    for( var i = 0;    i < _linkedBunches . Count;    i++ )
        //    {
        //        if ( _linkedBunches [ i ] . ContainsBunch ( possibleMember ) )
        //        {
        //            result = _linkedBunches [ i ];
        //        }
        //    }

        //    return result;
        //}


        //private void ConductRelationUntillRelativeIsRenderedOrAbsents ( List<IGenderRelative> leaves, IGenderRelative possibleDescendant, ParamNode nodeBetweenRelatives )
        //{
        //    while ( true )
        //    {
        //        if ( leaves . Contains ( possibleDescendant ) )
        //        {
        //            leaves . Remove ( possibleDescendant );
        //            break;
        //        }
                
        //        if ( possibleDescendant . _renderedOnRelation )
        //        {
        //            break;
        //        }

        //        nodeBetweenRelatives = nodeBetweenRelatives . _parent;
        //        var rootIsAchieved = ( nodeBetweenRelatives == null );

        //        if ( rootIsAchieved )
        //        {
        //            possibleDescendant . _renderedOnRelation = true;
        //            break;
        //        }

        //        SetUpAncestorIfItFound ( nodeBetweenRelatives , possibleDescendant );
        //    }
        //}


        //private void SetUpAncestorIfItFound ( ParamNode nodeBetweenRelatives ,  IGenderRelative possibleDescendant )
        //{
        //    IGenderRelative ancestor = null;

        //    if ( _nodeToCircuits . ContainsKey ( nodeBetweenRelatives ) )
        //    {
        //        var nodeInAncestor = nodeBetweenRelatives;
        //        var presentingAncestorCircuit = _nodeToCircuits [ nodeInAncestor ] [ 0 ];
        //        var accomplishedDescendant = possibleDescendant;
        //        FixRelative ( presentingAncestorCircuit , ancestor , nodeInAncestor );
        //        ancestor . AddChild ( accomplishedDescendant );
        //        accomplishedDescendant . SetParent ( ancestor );
        //        accomplishedDescendant . _renderedOnRelation = true;
        //        possibleDescendant = ancestor;
        //    }
        //    else
        //    {
        //        possibleDescendant . AddToWayToParent ( nodeBetweenRelatives );
        //    }
        //}


    }
}