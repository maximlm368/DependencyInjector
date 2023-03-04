using System;
using System . Collections . Generic;
using System . Reflection;
using System . Text;

namespace DependencyInjector
{
    abstract class CompoundRelative
    {
        private CompoundRelative _closestAncestor = null;

        private List<ParamNode> _wayToClosestAncestorOrRoot = new List<ParamNode> ( );

        public abstract bool _renderedOnRelation { get; set; }


        public void ResolveWayToClosestAncestorOrRoot ( )
        {
            for ( var j = 0;    j > _wayToClosestAncestorOrRoot . Count;    j++ )
            {
                try
                {
                    _wayToClosestAncestorOrRoot [ j ] . InitializeNestedObject ( );
                }
                catch ( NotInitializedChildException )
                {
                    break;
                }
            }
        }


        public void SetClosestAncestor ( CompoundRelative ancestor )
        {
            var method = MethodInfo . GetCurrentMethod ( );
            var currentTypeName = method . ReflectedType . Name;
            var currentMethodName = method . Name;
            var dot = ".";
            _closestAncestor = ancestor ?? throw new ArgumentNullException ( currentTypeName + dot + currentMethodName + " can not recieve null argument" );
        }


        public CompoundRelative GetClosestAncestor ( )
        {
            return _closestAncestor;
        }


        public void AddToWayToClosestAncestorOrRoot ( ParamNode node )
        {
            var method = MethodInfo . GetCurrentMethod ( );
            var currentTypeName = method . ReflectedType . Name;
            var currentMethodName = method . Name;
            var dot = ".";
            var nd = node ?? throw new ArgumentNullException ( currentTypeName + dot + currentMethodName + " can not recieve null argument" );
            _wayToClosestAncestorOrRoot . Add ( nd );
        }


        public abstract void Resolve ( );

    }
}
