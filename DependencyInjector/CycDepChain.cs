using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;

namespace DependencyInjector
{
    public class DependencyChain : IComparable
    {
        private class ParamWrapper 
        {
            public int _nodeCounter = 0;

            public List<DependencyChain> _updatedList = null;

            public List<DependencyChain> _obsoleteChainList = null;

            public bool _metSomeTop = false;

            public Dictionary<ParamNode , List<DependencyChain>> _nodeToChains { get; set; }

            public Dictionary<List<DependencyChain> , Bunch> _chainsToBunch { get; set; }
        }


        private static string _nullArg = "'map' param in 'RenderOnMap' can not be null";

        private List<ParamNode> _chain { get; set; }

        public ParamNode _top { get; private set; }

        public ParamNode _bottom { get; private set; }

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


        public void ResolveDependency ()
        {
            var beingProcessed = _bottom;

            while( true )
            {
                try
                {
                    beingProcessed . InitializeNestedObject ( );
                }
                catch( NotInitializedChildException )
                {
                    break;
                }

                var timeToGoOut = object . ReferenceEquals ( _top ,  beingProcessed . _parent );

                if ( timeToGoOut )
                {
                    break;
                }

                beingProcessed  =  beingProcessed . _parent;
            }

        }


        public void RenderOnMap ( Dictionary <ParamNode , List <DependencyChain>> nodeToChainList,   Dictionary <List <DependencyChain>, Bunch> chainListToBunch )
        {
            var paramWrapper = new ParamWrapper ( );
            paramWrapper . _nodeToChains  =  nodeToChainList  ??  throw new ArgumentNullException ( _nullArg );
            paramWrapper . _chainsToBunch  =  chainListToBunch  ??  throw new ArgumentNullException ( _nullArg );
            var canMeetFork = true;                                                                               // notice first node !!!!!!
            var chainContinues = true;
            paramWrapper . _metSomeTop = false;

            while ( chainContinues )
            {
                paramWrapper . _updatedList = null;

                for (  ;    paramWrapper . _nodeCounter  <  _chain . Count;    paramWrapper . _nodeCounter++ )
                {
                    paramWrapper . _obsoleteChainList = null;

                    if ( (paramWrapper . _nodeCounter == 0)   ||   paramWrapper . _metSomeTop )
                    {
                        HandleFirstNode ( paramWrapper );
                    }
                    else
                    {
                        HandleFollowingNodes ( paramWrapper , canMeetFork , chainContinues );
                    }
                }
            }
        }


        private void HandleFirstNode ( ParamWrapper parameters )
        {
            var nodeCounter = parameters . _nodeCounter;

            parameters . _metSomeTop  =  false;

            if ( parameters . _nodeToChains . ContainsKey ( _chain [ nodeCounter ] ) )
            {
                parameters . _obsoleteChainList  =  parameters . _nodeToChains [ _chain [ nodeCounter ] ];
                parameters . _updatedList  =  parameters . _nodeToChains [ _chain [ nodeCounter ] ] . Clone ( );
                parameters . _updatedList . Add ( this );
                parameters . _nodeToChains [ _chain [ nodeCounter ] ]  =  parameters . _updatedList;
            }
            else
            {
                parameters . _updatedList  =  new List<DependencyChain> ( ) { this };
                parameters . _nodeToChains . Add ( _chain [ nodeCounter ] , parameters . _updatedList );
            }
        }


        private void HandleFollowingNodes ( ParamWrapper parameters , bool canMeetFork , bool chainContinues )
        {
            var nodeCounter  =  parameters . _nodeCounter;
            HandleFollowingNodes ( parameters );

            var chainEnds  =  ( ( _chain . Count - 2 )   <   parameters . _nodeCounter );

            if ( chainEnds )
            {
                chainContinues = false;
            }
            else
            {
                var metSomeTop  =  parameters . _nodeToChains [ _chain [ nodeCounter ] ] . Count   <= parameters . _nodeToChains [ _chain [ nodeCounter + 1 ] ] . Count;

                if ( metSomeTop )
                {
                    canMeetFork = true;
                    parameters . _metSomeTop = true;
                    return;
                }

                var metFork  =  parameters . _nodeToChains [ _chain [ nodeCounter ] ] . Count   > parameters . _nodeToChains [ _chain [ nodeCounter + 1 ] ] . Count;

                if ( metFork   &&   canMeetFork )
                {
                    HandlePossibleFork ( parameters , canMeetFork );
                }
            }
        }
        

        private void HandleFollowingNodes ( ParamWrapper parameters )
        {
            var nodeCounter  =  parameters . _nodeCounter;

            if ( parameters . _nodeToChains . ContainsKey ( _chain [ nodeCounter ] ) )
            {
                parameters . _nodeToChains [ _chain [ nodeCounter ] ]  =  parameters . _updatedList;
            }
            else
            {
                parameters . _nodeToChains . Add ( _chain [ nodeCounter ] , parameters . _updatedList );
            }
        }


        private void HandlePossibleFork ( ParamWrapper parameters , bool canMeetFork )
        {
            canMeetFork = false;
            var nodeCounter  =  parameters . _nodeCounter;
            var node  =  _chain [ nodeCounter ];
            var renderedChains  =  parameters . _nodeToChains [ node ];
            _chain [ nodeCounter ] . ChangeState ( NodeKind . Fork );

            if ( (parameters . _obsoleteChainList != null)   &&   (parameters . _chainsToBunch . ContainsKey ( parameters . _obsoleteChainList )) )
            {
                var bunch  =  new Bunch ( renderedChains );
                parameters . _chainsToBunch . Add ( renderedChains , bunch );
                parameters . _chainsToBunch . Remove ( parameters . _obsoleteChainList );
            }
            else
            {
                var chains  =  new List<DependencyChain> ( );
                chains . AddRange ( renderedChains );
                parameters . _chainsToBunch . Add ( renderedChains , new Bunch ( chains ) );
            }
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