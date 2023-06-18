using System;
using System . Collections . Generic;
using System . Reflection;
using System . Text;

namespace DependencyResolver
{
    abstract class CompoundRelative
    {
        private CompoundRelative _closestAncestor = null;

        private List<ParamNode> _wayToClosestAncestorOrRoot = new List<ParamNode> ( );

        internal abstract bool _renderedOnRelation { get; set; }


        internal void ResolveWayToClosestAncestorOrRoot ( )
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


        internal void SetClosestAncestor ( CompoundRelative ancestor )
        {
            var method = MethodInfo . GetCurrentMethod ( );
            var currentTypeName = method . ReflectedType . Name;
            var currentMethodName = method . Name;
            var dot = ".";
            _closestAncestor = ancestor ?? throw new ArgumentNullException (currentTypeName + dot + currentMethodName + " can not recieve null argument");
        }


        internal CompoundRelative GetClosestAncestor ( )
        {
            return _closestAncestor;
        }


        internal void AddToWayToClosestAncestorOrRoot ( ParamNode node )
        {
            var method = MethodInfo . GetCurrentMethod ( );
            var currentTypeName = method . ReflectedType . Name;
            var currentMethodName = method . Name;
            var dot = ".";
            var nd = node ?? throw new ArgumentNullException ( currentTypeName + dot + currentMethodName + " can not recieve null argument" );
            _wayToClosestAncestorOrRoot . Add ( nd );
        }


        internal abstract void Resolve ( );

    }
}
