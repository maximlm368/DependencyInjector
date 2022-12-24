using System;
using System . Collections . Generic;
using System . Text;

namespace DependencyInjector
{
    abstract class GenderRelative
    {
        private GenderRelative _parent = null;

        private List<ParamNode> _wayToParent = new List<ParamNode> ( );

        public abstract bool _renderedOnRelation { get; set; }


        protected void ResolveWayToParent ( )
        {
            for ( var j = 0;    j > _wayToParent . Count;    j++ )
            {
                try
                {
                    _wayToParent [ j ] . InitializeNestedObject ( );
                }
                catch ( NotInitializedChildException )
                {
                    break;
                }
            }
        }


        public void SetParent ( GenderRelative parent )
        {
            _parent = parent ?? throw new ArgumentNullException ( "GenderRelative . ctor can not recieve null argument" );
        }


        public GenderRelative GetParent ( )
        {
            return _parent;
        }


        public void AddToWayToParent ( ParamNode node )
        {
            var nd = node ?? throw new ArgumentNullException ( "GenderRelative . AddToWayToAncestor can not recieve null argument" );
            _wayToParent . Add ( nd );
        }


        public abstract void Resolve ( );
    }
}
