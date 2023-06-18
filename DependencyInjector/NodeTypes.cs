using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Reflection;

namespace DependencyResolver
{
    abstract class NodeType
    {
        internal NodeKind _kind;

        internal abstract void InitializeNested ( ParamNode becomingInitialized );

        internal virtual List<ParamNode> DefineChildren ( ParamNode becomingParent , List<Type> childParamTypes )
        {
            return new List<ParamNode> ( );
        }
    }



    class OrdinaryNode : NodeType
    {
        internal new NodeKind _kind = NodeKind . Ordinary;

        
        internal override void InitializeNested ( ParamNode becomingInitialized )
        {
            becomingInitialized . InitializeOrdinary ( );
        }


        internal override List<ParamNode> DefineChildren ( ParamNode becomingParent , List<Type> childTypes )
        {
            var resultChildren = new List<ParamNode> ( );

            for ( var counter = 0;     counter < childTypes . Count;     counter++ )
            {
                var nodeType = DefineNodeType ( childTypes [ counter ] );
                var child = new ParamNode ( childTypes [ counter ] , counter , becomingParent , nodeType );
                resultChildren . Add ( child );
            }

            return resultChildren;
        }


        private NodeType DefineNodeType ( Type paramType )
        {
            NodeType result;
            bool isSimple = ( paramType . IsPrimitive )   ||   ( paramType . FullName == "System.String" );

            if ( isSimple )
            {
                result = new SimpleLeaf ( );
            }
            else
            {
                result = new OrdinaryNode ( );
            }

            return result;
        }
    }



    class SimpleLeaf : NodeType
    {
        internal new NodeKind _kind = NodeKind . SimpleLeaf;

        internal override void InitializeNested ( ParamNode becomingInitialized )
        {
            becomingInitialized . InitializeSimple ( );
        }
    }



    class DependencyCycleParticipant : NodeType
    {
        internal new NodeKind _kind = NodeKind . DependencyCycleParticipant;

        private bool _ordinaryChildrenMustBeInitializedAlready = false;


        internal override void InitializeNested ( ParamNode becomingInitialized )
        {
            Performer performer = becomingInitialized . InitializeOrdinary;
            Initialize ( performer );
        }


        protected void Initialize ( Performer performer)
        {
            if ( _ordinaryChildrenMustBeInitializedAlready )
            {
                performer ( );
            }
            else
            {
                _ordinaryChildrenMustBeInitializedAlready = true;
                return;
            }
        }
    }



    class TopOfCircuit : DependencyCycleParticipant
    {
        internal new NodeKind _kind = NodeKind . TopOfCircuit;


        internal override void InitializeNested ( ParamNode becomingInitialized )
        {
            Performer performer = becomingInitialized . InitializePassingOverAbsentParams;
            base. Initialize ( performer );
        }
    }



    class BottomOfCircuit : DependencyCycleParticipant
    {
        internal new NodeKind _kind = NodeKind . BottomOfCircuit;


        internal override void InitializeNested ( ParamNode becomingInitialized )
        {
            Performer performer = becomingInitialized . InitializeByTemplate;
            base . Initialize ( performer );
        }
    }



    enum NodeKind
    {
        Ordinary,

        DependencyCycleParticipant,

        SimpleLeaf,

        TopOfCircuit,

        BottomOfCircuit
    }



    delegate void Performer ( );

}