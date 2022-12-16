using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;

namespace DependencyInjector
{
    public class DependencyChain : IComparable, IGenderTreeNode
    {
        private class ParamWrapper 
        {
            public int _nodeCounter = 0;

            public List<DependencyChain> _updatedStack = null;

            public List<DependencyChain> _obsoleteStack = null;

            public bool _metSomeTop = false;

            public bool _canRegesterBunch = true;

            public bool _chainContinues = true;

            public bool _isStartAfterFork = false;

            public Dictionary<ParamNode , List<DependencyChain>> _nodeToChains { get; set; }

            public Dictionary<List<DependencyChain> , Bunch> _chainsToBunch { get; set; }
        }


        private static string _nullArg = "'map' param in 'RenderOnMap' can not be null";

        private static int _idCounter = 0;

        private bool _complited = false;

        private ParamNode _beingProcessed = null;

        private List<ParamNode> _chain { get; set; }

        public bool _isBunched { get; set; }

        public int _id { get; private set; }

        public ParamNode _top { get; private set; }

        public ParamNode _bottom { get; private set; }

        public bool _renderedOnRelation { get; set; }

        // private bool _IsEmpty { get; set; }


        public DependencyChain ( )
        {
           // _IsEmpty = true;
        }


        public DependencyChain ( List<ParamNode> nodes )
        {
            if ( nodes . Count < 2 )
            {
                throw new Exception ( " );
            }

            VerifyChainConsistency ( nodes );
            VerifyTopBottomTypeCoincidence ( nodes );
            _chain = nodes;
            _bottom = _chain . First ( );
            _top = _chain . Last ( );
            _isBunched = false;
            _id = _idCounter;
            _idCounter++;

            // _IsEmpty = false;

        }


        public static DependencyChain CreateEmptyChain ( )
        {
            return new DependencyChain ( );
        }


        private void VerifyChainConsistency ( List<ParamNode> nodes )
        {
            for ( var i = 0; i < nodes . Count - 1; i++ )
            {
                bool parentIsCorrect = Object . ReferenceEquals ( nodes [ i ] . _parent , nodes [ i + 1 ] );

                if ( ( nodes [ i ] != null ) && !parentIsCorrect )
                {
                    throw new Exception ( " );
                }
            }
        }


        private void VerifyTopBottomTypeCoincidence ( List<ParamNode> nodes )
        {
            if ( nodes . First ( ) . _nestedObj . GetObjectTypeName ( ) != nodes . Last ( ) . _nestedObj . GetObjectTypeName ( ) )
            {
                throw new Exception ( " );
            }
        }


        //public void Render ( List<ParamNode> [ ] store )
        //{
        //    for ( var i = 0;   i < _chain . Count;   i++ )
        //    {
        //        store [ _chain [ i ] . _myLevelInTree ] . Add ( _chain [ i ] );
        //    }
        //}


        int IComparable.CompareTo ( object obj )
        {
            var result = 0;

            DependencyChain comparable;

            if ( !( obj is DependencyChain ) )
            {
                throw new Exception ( ");
            }
            else
            {
                comparable = ( DependencyChain ) obj;
            }

            result = _top . _myLevelInTree - comparable . _top . _myLevelInTree;

            return result;
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


        public Bunch GetBunchYouAreBunchedIn ( ParamNode beingProcessed, 
                                               Dictionary <ParamNode, List<DependencyChain>> nodeToChainStack ,   Dictionary <List<DependencyChain>, Bunch> chainStackToBunch )
        {
            var currentIndex  =  _chain . IndexOf ( beingProcessed );
            Bunch bunch = null;

            for ( ;   currentIndex  >  _chain . Count;    currentIndex ++ )
            {
                if ( _chain [ currentIndex ] . _nodeType . _kind  ==  NodeKind . Fork )
                {
                    try
                    {
                        bunch = GetBunch ( beingProcessed , nodeToChainStack , chainStackToBunch );
                    }
                    catch( System . Collections . Generic . KeyNotFoundException )
                    {
                        break;
                    }
                }
            }

            for ( ;    currentIndex  >  _chain . Count;    currentIndex -- )
            {
                try
                {
                    bunch = GetBunch ( beingProcessed , nodeToChainStack , chainStackToBunch );
                }
                catch ( System . Collections . Generic . KeyNotFoundException )
                {

                }
            }

            if( bunch == null )
            {
                throw new NotBunchedChainException;
            }

            return bunch;
        }


        private Bunch GetBunch ( ParamNode beingProcessed ,
                                               Dictionary<ParamNode , List<DependencyChain>> nodeToChainStack , Dictionary<List<DependencyChain> , Bunch> chainStackToBunch )
        {
            Bunch bunch = null;
            var chainStack = nodeToChainStack [ beingProcessed ];
            bunch = chainStackToBunch [ chainStack ];
            return bunch;
        }


        public void ResolveDependency ()
        {
            if( ! _isBunched   &&   ! _complited )
            {
                if( _beingProcessed == null )                
                {
                    _top . InitializeNestedObject ( );
                    InitializeBottomByTop ( );
                    _beingProcessed = _bottom;
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

                    var timeToGoOut = object . ReferenceEquals ( _top ,  _beingProcessed . _parent );

                    if ( timeToGoOut )
                    {
                        _complited = true;
                        break;
                    }

                    _beingProcessed  =  _beingProcessed . _parent;
                }
            }
        }


        public void RenderOnMap ( Dictionary <ParamNode , List <DependencyChain>> nodeToChainStack,   Dictionary <List <DependencyChain>, Bunch> chainStackToBunch )
        {
            var paramWrapper = new ParamWrapper ( );
            paramWrapper . _nodeToChains  =  nodeToChainStack  ??  throw new ArgumentNullException ( _nullArg );
            paramWrapper . _chainsToBunch  =  chainStackToBunch  ??  throw new ArgumentNullException ( _nullArg );
            paramWrapper . _updatedStack = null;
            paramWrapper . _obsoleteStack = null;

            while ( paramWrapper . _chainContinues )
            {
                if ( ( paramWrapper . _nodeCounter == 0 )   ||   paramWrapper . _metSomeTop   ||   paramWrapper . _isStartAfterFork )
                {
                    HandleFirstNode ( paramWrapper );
                }
                else
                {
                    HandlePlainNode ( paramWrapper );
                    DetectPrecedingStackHeightChange ( paramWrapper );
                }

                paramWrapper . _nodeCounter++;

                for (  ;    paramWrapper . _nodeCounter  <  _chain . Count;    paramWrapper . _nodeCounter++ )
                {

                }
            }
        }


        private void HandleFirstNode ( ParamWrapper parameters )
        {
            var nodeCounter = parameters . _nodeCounter;
            parameters . _metSomeTop  =  false;

            if ( parameters . _nodeToChains . ContainsKey ( _chain [ nodeCounter ] ) )
            {
                parameters . _obsoleteStack  =  parameters . _nodeToChains [ _chain [ nodeCounter ] ];
                parameters . _updatedStack  =  parameters . _nodeToChains [ _chain [ nodeCounter ] ] . Clone ( );
                parameters . _updatedStack . Add ( this );
                parameters . _nodeToChains [ _chain [ nodeCounter ] ]  =  parameters . _updatedStack;
            }
            else
            {
                parameters . _updatedStack  =  new List<DependencyChain> ( ) { this };
                parameters . _nodeToChains . Add ( _chain [ nodeCounter ] ,  parameters . _updatedStack );
            }

            parameters . _isStartAfterFork = false;
            DetectPrecedingStackHeightChange ( parameters );
        }
        

        private void HandlePlainNode ( ParamWrapper parameters )
        {
            var nodeCounter  =  parameters . _nodeCounter;

            if ( parameters . _nodeToChains . ContainsKey ( _chain [ nodeCounter ] ) )
            {
                parameters . _nodeToChains [ _chain [ nodeCounter ] ]  =  parameters . _updatedStack;
            }
            else
            {
                parameters . _nodeToChains . Add ( _chain [ nodeCounter ] ,  parameters . _updatedStack );
            }

        }


        private void DetectPrecedingStackHeightChange ( ParamWrapper parameters )
        {
            var nodeCounter = parameters . _nodeCounter;

            if ( _chain . Count   >   (nodeCounter + 1) )
            {
                var nextIsSomeTop = ( parameters . _nodeToChains [ _chain [ nodeCounter ] ] . Count )  <=  
                                    ( parameters . _nodeToChains [ _chain [ nodeCounter + 1 ] ] . Count );

                if ( nextIsSomeTop )
                {
                    parameters . _canRegesterBunch = true;
                    parameters . _metSomeTop = true;
                    return;
                }

                var isNewFork = DetectFork ( parameters );

                if ( isNewFork )
                {
                    HandleFork ( parameters );
                }
            }
            else
            {
                parameters . _chainContinues = false;
            }
        }


        private bool DetectFork ( ParamWrapper parameters )
        {
            var nodeCounter = parameters . _nodeCounter;
            var isNewFork = false;

            try
            {
                isNewFork = ( 
                               ( parameters . _nodeToChains [ _chain [ nodeCounter ] ] . Count )   >   1 
                            ) 
                              &&
                            ( 
                               ( parameters . _nodeToChains [ _chain [ nodeCounter ] ] . Count ) 
                                  > 
                               ( parameters . _nodeToChains [ _chain [ nodeCounter + 1 ] ] . Count + 1 ) 
                            );
            }
            catch ( System . Collections . Generic . KeyNotFoundException )
            {
                // it means dictionary 'parameters . _nodeToChains'  does not contains next node '[ _chain [ nodeCounter + 1 ] ]'

                isNewFork = true;
            }

            return isNewFork;
        }


        private void HandleFork ( ParamWrapper parameters )
        {
            var nodeCounter  =  parameters . _nodeCounter;
            var node  =  _chain [ nodeCounter ];
            var renderedChains  =  parameters . _nodeToChains [ node ];
            _chain [ nodeCounter ] . ChangeState ( NodeKind . Fork );

            if ( parameters . _canRegesterBunch )
            {
                if ( ( parameters . _obsoleteStack != null )   &&   ( parameters . _chainsToBunch . ContainsKey ( parameters . _obsoleteStack ) ) )
                {
                    var bunch = new Bunch ( renderedChains );
                    parameters . _chainsToBunch . Add ( renderedChains , bunch );
                    parameters . _chainsToBunch . Remove ( parameters . _obsoleteStack );
                }
                else
                {
                    var chains = new List<DependencyChain> ( );
                    chains . AddRange ( renderedChains );
                    parameters . _chainsToBunch . Add ( renderedChains , new Bunch ( chains ) );
                }

                parameters . _canRegesterBunch = false;
            }

            parameters . _isStartAfterFork = true;
        }



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