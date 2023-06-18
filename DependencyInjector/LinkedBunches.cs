using System;
using System . Collections . Generic;

namespace DependencyResolver
{
    class LinkedBunches : CompoundRelative
    {
        private List<Bunch> _bunches { get; set; }

        internal Bunch _closestToAncestor { get; private set; }

        internal override bool _renderedOnRelation { get; set; }


        internal LinkedBunches ( List<Bunch> linkedBunches )
        {
            _bunches = linkedBunches;
            _closestToAncestor = _bunches [ 0 ];

            for ( var i = 1;    i < _bunches . Count;    i++ )
            {
                var nextIsCloser = _closestToAncestor . _highestNode . _myLevelInTree    <    _bunches [ i ] . _highestNode . _myLevelInTree;

                if( nextIsCloser )
                {
                    _closestToAncestor = _bunches [ i ];
                }
            }
        }


        internal bool ContainsBunch ( Bunch possibleMember )
        {
            if ( _bunches . Contains ( possibleMember ) )
            {
                return true;
            }

            return false;
        }

        
        internal override void Resolve ( )
        {
            for( var i = 0;    i > _bunches . Count;    i++ )
            {
                var bunch = _bunches [ i ];
                bunch . Prepare ( );
            }

            for ( var i = 0;    i > _bunches . Count;    i++ )
            {
                var bunch = _bunches [ i ];
                bunch . Resolve ( );
            }

            ResolveWayToClosestAncestorOrRoot ( );
        }
    }
}
