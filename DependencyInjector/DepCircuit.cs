using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;

namespace DependencyInjector
{
    class DependencyCircuit : GenderRelative, IComparable
    {
        private static string _topDoesNotCoincideBottom = "first item in arg must coincide last";

        private static int _idCounter = 0;

        private bool _complited;

        private ParamNode _beingProcessed;

        private List<ParamNode> _nodeChain;

        public int _id { get; private set; }

        public ParamNode _top { get; private set; }

        public ParamNode _bottomCoincidencesTop { get; private set; }

        public override bool _renderedOnRelation { get; set; }

        public bool _isBunched { get; set; }
        

        public DependencyCircuit ( List<ParamNode> nodes )
        {
            if ( nodes . Count < 2 )
            {
                throw new Exception ( " );
            }

            VerifyChainConsistency ( nodes );
            VerifyTopBottomTypeCoincidence ( nodes );
            _nodeChain = nodes . Clone ( );
            _bottomCoincidencesTop = _nodeChain . First ( );
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

                if ( ( nodes [ i ] != null )     &&     !parentIsCorrect )
                {
                    throw new Exception ( " );
                }
            }
        }


        private void VerifyTopBottomTypeCoincidence ( List<ParamNode> nodes )
        {
            if ( ! nodes . First ( ) . TypeNameEquals ( nodes . Last ( ) ) )
            {
                throw new Exception ( _topDoesNotCoincideBottom );
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


        public void InitializeBottomByTop ()
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


        //public Bunch GetBunchYouAreBunchedIn ( ParamNode beingProcessed, 
        //                                       Dictionary <ParamNode, List<DependencyCircuit>> nodeToChainStack ,   Dictionary <List<DependencyCircuit>, Bunch> chainStackToBunch )
        //{
        //    var currentIndex  =  _nodeChain . IndexOf ( beingProcessed );
        //    Bunch bunch = null;

        //    for ( ;   currentIndex  >  _nodeChain . Count;    currentIndex ++ )
        //    {
        //        if ( _nodeChain [ currentIndex ] . _nodeType . _kind  ==  NodeKind . Fork )
        //        {
        //            try
        //            {
        //                bunch = GetBunch ( beingProcessed , nodeToChainStack , chainStackToBunch );
        //            }
        //            catch( System . Collections . Generic . KeyNotFoundException )
        //            {
        //                break;
        //            }
        //        }
        //    }

        //    for ( ;    currentIndex  >  _nodeChain . Count;    currentIndex -- )
        //    {
        //        try
        //        {
        //            bunch = GetBunch ( beingProcessed , nodeToChainStack , chainStackToBunch );
        //        }
        //        catch ( System . Collections . Generic . KeyNotFoundException )
        //        {

        //        }
        //    }

        //    if( bunch == null )
        //    {
        //        throw new NotBunchedChainException;
        //    }

        //    return bunch;
        //}


        //private Bunch GetBunch ( ParamNode beingProcessed ,
        //                                       Dictionary<ParamNode , List<DependencyCircuit>> nodeToChainStack , Dictionary<List<DependencyCircuit> , Bunch> chainStackToBunch )
        //{
        //    Bunch bunch = null;
        //    var chainStack = nodeToChainStack [ beingProcessed ];
        //    bunch = chainStackToBunch [ chainStack ];
        //    return bunch;
        //}


        public override void Resolve ()
        {
            if( ! _complited )
            {
                if( _beingProcessed == null )                
                {
                    _top . InitializeNestedObject ( );
                    InitializeBottomByTop ( );
                    _beingProcessed = _bottomCoincidencesTop;
                }

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

                    var timeToGoOut = _top . Equals ( _beingProcessed . _parent );

                    if ( timeToGoOut )
                    {
                        _complited = true;
                        break;
                    }

                    _beingProcessed  =  _beingProcessed . _parent;
                }

                ResolveWayToParent ( );
            }
        }

        //public override void Resolve ( )
        //{
        //    throw new NotImplementedException ( );
        //}



        //public void RenderOnMap ( Dictionary <ParamNode , List <DependencyCircuit>> nodeToChainStack,   Dictionary <List <DependencyCircuit>, Bunch> chainStackToBunch )
        //{
        //    var paramWrapper = new ParamWrapper ( );
        //    paramWrapper . _nodeToChains  =  nodeToChainStack  ??  throw new ArgumentNullException ( _nullArg );
        //    paramWrapper . _chainsToBunch  =  chainStackToBunch  ??  throw new ArgumentNullException ( _nullArg );
        //    paramWrapper . _updatedStack = null;
        //    paramWrapper . _obsoleteStack = null;

        //    while ( paramWrapper . _chainContinues )
        //    {
        //        if ( ( paramWrapper . _nodeCounter == 0 )   ||   paramWrapper . _metSomeTop   ||   paramWrapper . _isStartAfterFork )
        //        {
        //            HandleFirstNode ( paramWrapper );
        //        }
        //        else
        //        {
        //            HandlePlainNode ( paramWrapper );
        //            DetectPrecedingStackHeightChange ( paramWrapper );
        //        }

        //        paramWrapper . _nodeCounter++;

        //        for (  ;    paramWrapper . _nodeCounter  <  _nodeChain . Count;    paramWrapper . _nodeCounter++ )
        //        {

        //        }
        //    }
        //}


        //private void HandleFirstNode ( ParamWrapper parameters )
        //{
        //    var nodeCounter = parameters . _nodeCounter;
        //    parameters . _metSomeTop  =  false;

        //    if ( parameters . _nodeToChains . ContainsKey ( _nodeChain [ nodeCounter ] ) )
        //    {
        //        parameters . _obsoleteStack  =  parameters . _nodeToChains [ _nodeChain [ nodeCounter ] ];
        //        parameters . _updatedStack  =  parameters . _nodeToChains [ _nodeChain [ nodeCounter ] ] . Clone ( );
        //        parameters . _updatedStack . Add ( this );
        //        parameters . _nodeToChains [ _nodeChain [ nodeCounter ] ]  =  parameters . _updatedStack;
        //    }
        //    else
        //    {
        //        parameters . _updatedStack  =  new List<DependencyCircuit> ( ) { this };
        //        parameters . _nodeToChains . Add ( _nodeChain [ nodeCounter ] ,  parameters . _updatedStack );
        //    }

        //    parameters . _isStartAfterFork = false;
        //    DetectPrecedingStackHeightChange ( parameters );
        //}


        //private void HandlePlainNode ( ParamWrapper parameters )
        //{
        //    var nodeCounter  =  parameters . _nodeCounter;

        //    if ( parameters . _nodeToChains . ContainsKey ( _nodeChain [ nodeCounter ] ) )
        //    {
        //        parameters . _nodeToChains [ _nodeChain [ nodeCounter ] ]  =  parameters . _updatedStack;
        //    }
        //    else
        //    {
        //        parameters . _nodeToChains . Add ( _nodeChain [ nodeCounter ] ,  parameters . _updatedStack );
        //    }

        //}


        //private void DetectPrecedingStackHeightChange ( ParamWrapper parameters )
        //{
        //    var nodeCounter = parameters . _nodeCounter;

        //    if ( _nodeChain . Count   >   (nodeCounter + 1) )
        //    {
        //        var nextIsSomeTop = ( parameters . _nodeToChains [ _nodeChain [ nodeCounter ] ] . Count )  <=  
        //                            ( parameters . _nodeToChains [ _nodeChain [ nodeCounter + 1 ] ] . Count );

        //        if ( nextIsSomeTop )
        //        {
        //            parameters . _canRegesterBunch = true;
        //            parameters . _metSomeTop = true;
        //            return;
        //        }

        //        var isNewFork = DetectFork ( parameters );

        //        if ( isNewFork )
        //        {
        //            HandleFork ( parameters );
        //        }
        //    }
        //    else
        //    {
        //        parameters . _chainContinues = false;
        //    }
        //}


        //private bool DetectFork ( ParamWrapper parameters )
        //{
        //    var nodeCounter = parameters . _nodeCounter;
        //    var isNewFork = false;

        //    try
        //    {
        //        isNewFork = ( 
        //                       ( parameters . _nodeToChains [ _nodeChain [ nodeCounter ] ] . Count )   >   1 
        //                    ) 
        //                      &&
        //                    ( 
        //                       ( parameters . _nodeToChains [ _nodeChain [ nodeCounter ] ] . Count ) 
        //                          > 
        //                       ( parameters . _nodeToChains [ _nodeChain [ nodeCounter + 1 ] ] . Count + 1 ) 
        //                    );
        //    }
        //    catch ( System . Collections . Generic . KeyNotFoundException )
        //    {
        //        // it means dictionary 'parameters . _nodeToChains'  does not contain next node '[ _chain [ nodeCounter + 1 ] ]'

        //        isNewFork = true;
        //    }

        //    return isNewFork;
        //}


        //private void HandleFork ( ParamWrapper parameters )
        //{
        //    var nodeCounter  =  parameters . _nodeCounter;
        //    var node  =  _nodeChain [ nodeCounter ];
        //    var renderedChains  =  parameters . _nodeToChains [ node ];
        //    _nodeChain [ nodeCounter ] . ChangeState ( NodeKind . Fork );

        //    if ( parameters . _canRegesterBunch )
        //    {
        //        if ( ( parameters . _obsoleteStack != null )   &&   ( parameters . _chainsToBunch . ContainsKey ( parameters . _obsoleteStack ) ) )
        //        {
        //            var bunch = new Bunch ( renderedChains );
        //            parameters . _chainsToBunch . Add ( renderedChains , bunch );
        //            parameters . _chainsToBunch . Remove ( parameters . _obsoleteStack );
        //        }
        //        else
        //        {
        //            var chains = new List<DependencyCircuit> ( );
        //            chains . AddRange ( renderedChains );
        //            parameters . _chainsToBunch . Add ( renderedChains , new Bunch ( chains ) );
        //        }

        //        parameters . _canRegesterBunch = false;
        //    }

        //    parameters . _isStartAfterFork = true;
        //}



        //private int Compare ( DependencyChain comparable )
        //{
        //    var result = this . top . myLevelInTree - comparable . top . myLevelInTree;

        //    return result;
        //}


        //public void Add ( ParamNode node )
        //{
        //    if( node != null )
        //    {
        //        this . chain . Add ( node );

        //        if ( this . state == ChainStait . Disabled )
        //        {
        //            this . state = ChainStait . Enabled;
        //        }
        //    }

        //}



        //public void Enable ()
        //{
        //    for ( var i = 0;    i < this . chain . Count - 1;    i++ )
        //    {
        //        this . chain [ i ] . ResetParentState ( );
        //    }

        //   // this . state = ChainStait . Enabled;
        //}



    }

}