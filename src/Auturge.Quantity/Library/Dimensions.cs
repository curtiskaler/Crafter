// ReSharper disable InconsistentNaming

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable StaticMemberInitializerReferesToMemberBelow

using System.Diagnostics.CodeAnalysis;

namespace Auturge.Quantity;

public sealed class Dimensions : MemberCache<Dimensions, Dimension>
{
    #region List Methods

    public static Dimension FindOrAdd(DimensionVector vector)
    {
        var found = TryFind(vector, out var dimension);
        if (found && dimension != null)
        {
            return dimension;
        }

        // one doesn't exist, so let's wrap the vector up in a new Dimension
        var dim = new Dimension(vector.Analysis, vector.Analysis, vector);
        Add(dim);
        return dim;
    }

    public static Dimension Find(DimensionVector vector)
    {
        var found = TryFind(vector, out var dimension);
        if (found && dimension != null)
        {
            return dimension;
        }

        throw new KeyNotFoundException($"Dimension not found for vector [{vector.Analysis}].");
    }

    public static bool TryFind(DimensionVector vector, [MaybeNullWhen(false)] out Dimension dimension)
    {
        dimension = Items.FirstOrDefault(x => x.Equals(vector));
        return dimension != null;
    }

    #endregion List Methods

    // =======================================================================
    //   DIMENSIONS 
    // =======================================================================

    // ANY: A Dimension that corresponds to ANY dimension
    public static readonly Dimension Any = new(-1, "1", "", 0, 0, 0, 0, 0, 0, 0);
    
    // NONE: A dimension that corresponds to NO dimension
    public static readonly Dimension None = new(0, "", "", 0, 0, 0, 0, 0, 0, 0);
    
    // ONE: A dimensionless dimension, useful in multiplying/dividing dimensions.
    public static readonly Dimension One = new(1, "dimensions.one", string.Empty, DimensionVector.One);
    
    // Base Unit Types
    public static readonly Dimension Time = new("dimensions.si.time", "T", 1, 0, 0, 0, 0, 0, 0);
    public static readonly Dimension Length = new("dimensions.si.length", "L", 0, 1, 0, 0, 0, 0, 0);
    public static readonly Dimension Mass = new("dimensions.si.mass", "M", 0, 0, 1, 0, 0, 0, 0);

    public static readonly Dimension
        ElectricCurrent = new("dimensions.si.electric_current", "I", 0, 0, 0, 1, 0, 0, 0); // A
    public static readonly Dimension Current = ElectricCurrent; // A

    public static readonly Dimension Temperature = new("dimensions.si.temperature", "Θ", 0, 0, 0, 0, 1, 0, 0);
    public static readonly Dimension Amount = new("dimensions.si.amount", "N", 0, 0, 0, 0, 0, 1, 0);

    public static readonly Dimension LuminousIntensity =
        new("dimensions.si.luminous_intensity", "J", 0, 0, 0, 0, 0, 0, 1);

    // "synonyms"
    public static readonly Dimension Ratio = One.AddSynonym("dimensions.ratio", string.Empty);
    public static readonly Dimension Distance = Length.AddSynonym("dimensions.distance", "d");


    // =======================================================================
    //   DERIVED DIMENSIONS 
    // =======================================================================
    public static readonly Dimension Area = new("dimensions.area", "A", Length * Length);
    public static readonly Dimension Volume = new("dimensions.volume", "V", Length * Length * Length);
    public static readonly Dimension Frequency = new("dimensions.frequency", "f", One / Time);
    public static readonly Dimension Angle = new("dimensions.angle", "", Length / Length);
    public static readonly Dimension SolidAngle = new("dimensions.solid_angle", "", (Area) / (Area));


    public static readonly Dimension Density = new("dimensions.density", "d", Mass / Volume);

    public static readonly Dimension Velocity = new("dimensions.velocity", "v", Length / Time);
    public static readonly Dimension Acceleration = new("dimensions.acceleration", "a", Velocity / Time);
    public static readonly Dimension Jerk = new("dimensions.jerk", "j", Acceleration / Time);
    public static readonly Dimension Jolt = Jerk.AddSynonym("dimensions.jolt", "j");
    public static readonly Dimension Snap = new("dimensions.snap", "s", Jerk / Time);
    public static readonly Dimension Jounce = Snap.AddSynonym("dimensions.jounce", "s");

    public static readonly Dimension Force = new("dimensions.force", "F", Mass * Length / (Time * Time)); // N
    public static readonly Dimension Weight = Force.AddSynonym("dimensions.weight", "W");

    public static readonly Dimension Yank = new("dimensions.yank", "[M L T⁻³]", Force / Time);


    public static readonly Dimension Pressure = new("dimensions.pressure", "p", Force / (Length * Length));
    public static readonly Dimension Stress = Pressure.AddSynonym("dimensions.stress", "σ");

    public static readonly Dimension Energy = new("dimensions.energy", "e", Force * Length);
    public static readonly Dimension Work = Energy.AddSynonym("dimensions.work", "W");
    public static readonly Dimension Heat = Energy.AddSynonym("dimensions.heat", "Q");

    public static readonly Dimension Power = new("dimensions.power", "P", Force * Length / Time);
    public static readonly Dimension RadiantFlux = Power.AddSynonym("dimensions.electric.radiant_flux", "Φe");

    public static readonly Dimension
        ElectricCharge = new("dimensions.electric.charge", "Q", ElectricCurrent * Time); // C, A*h

    public static Dimension Charge => ElectricCharge;


    // Can we make these THREE reflect the same unit type?
    public static readonly Dimension Voltage = new("dimensions.voltage", "V", Work / Charge); // V
    public static readonly Dimension ElectricPotential = Voltage.AddSynonym("dimensions.electric.potential", "Δφ");
    public static readonly Dimension ElectromotiveForce = Voltage.AddSynonym("dimensions.electromotive_force", "E");

    // Electric units
    public static readonly Dimension
        ElectricPower = Power.AddSynonym("dimensions.electric.power", "P"); // Energy / Time = Force * Velocity,

    public static readonly Dimension
        Resistance = new("dimensions.electric.resistance", "R", ElectromotiveForce / Charge); // Ohms

    public static readonly Dimension Impedance = Resistance.AddSynonym("dimensions.electric.impedance", "Z");
    public static readonly Dimension Reactance = Resistance.AddSynonym("dimensions.electric.reactance", "X");


    public static readonly Dimension ApparentPower =
        new("dimensions.electric.apparent_power", "S", Voltage * Current);

    public static readonly Dimension
        ReactivePower = ApparentPower.AddSynonym("dimensions.electric.reactive_power", "Q");

    public static readonly Dimension ComplexPower = ApparentPower.AddSynonym("dimensions.electric.complex_power", "S");

    public static readonly Dimension
        Capacitance = new("dimensions.electric.capacitance", "C", Charge / Voltage); // C/V

    public static readonly Dimension Inductance = new("dimensions.electric.inductance", "L",
        Voltage * Time / Current); // V*s/A

    public static readonly Dimension
        Conductance = new("dimensions.electric.conductance", "G", Current / Voltage); // A/V

    public static readonly Dimension Permittivity = new("dimensions.electric.permittivity", "ε",
        Conductance / Distance);


    // admittance is reciprocal of impedance (Z)
    public static readonly Dimension
        Admittance = Conductance.AddSynonym("dimensions.electric.admittance", "Y"); // F/m = C/Vm = A2 S4 / M L3

    // reciprocal of conductivity
    public static readonly Dimension Resistivity = new("dimensions.electric.resistivity", "ρ", Resistance * Time);

    public static readonly Dimension Conductivity = new("dimensions.electric.conductivity", "σ", One / Resistivity);
    public static readonly Dimension ElectricField = new("dimensions.electric.field", "E", ElectricPotential / Length);

    public static readonly Dimension ElectricFlux = new("dimensions.electric.flux", "Φe", ElectricPotential * Length);

    public static readonly Dimension ElectronMobility =
        new("dimensions.electron.mobility", "μ", Area / (ElectromotiveForce * Time));

    public static readonly Dimension Exposure = new("dimensions.radiation.exposure", "D", ElectricCharge / Mass);
    public static readonly Dimension ExposureRate = new("dimensions.radiation.exposure_rate", "F", Exposure / Time);


    public static readonly Dimension LinearChargeDensity =
        new("dimensions.electric.linear_charge_density", "λ", ElectricCharge / Length);

    public static Dimension ChargeDensity => LinearChargeDensity;

    public static readonly Dimension SurfaceChargeDensity =
        new("dimensions.electric.surface_charge_density", "σ", ElectricCharge / Area);

    public static readonly Dimension
        ElectricFluxDensity = SurfaceChargeDensity.AddSynonym("dimensions.electric.flux_density", "D");

    public static readonly Dimension
        PoliarizationDensity = SurfaceChargeDensity.AddSynonym("dimensions.electric.polarization_density", "P");

    public static readonly Dimension
        VolumeChargeDensity = new("dimensions.electric.volume_charge_density", "ρ", ElectricCharge / Volume);


    // Magenetic units
    public static readonly Dimension MagneticFlux = new("dimensions.magnetic.flux", "ΦB", Force * Length / Current);

    public static readonly Dimension MagneticFluxDensity =
        new("dimensions.magnetic.flux_density", "B", Mass / (Time * Time * Current));

    public static readonly Dimension MagneticFieldStrength =
        MagneticFluxDensity.AddSynonym("dimensions.magnetic.field_strength", "B");

    public static readonly Dimension MagneticRigidity = new("dimensions.magnetic.rigidity", "Bρ", Resistance * Length);

    public static readonly Dimension MagneticInductance =
        new("dimensions.magnetic.inductance", "L", Force * Length / (Current * Current));

    public static readonly Dimension MagneticPermeability =
        new("dimensions.magnetic.permeability", "μ", MagneticInductance / Length);

    public static readonly Dimension MagneticReluctance =
        new("dimensions.magnetic.reluctance", "R", Length / (MagneticPermeability * Area));

    public static readonly Dimension
        MagneticPermeance = new("dimensions.magnetic.permeance", "𝒫", One / MagneticReluctance);

    // public static readonly Dimension MagneticConductance = new("dimensions.magnetic.conductance", "G", One / MagneticResistance);
}
