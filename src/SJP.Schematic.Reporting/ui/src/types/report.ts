/**
 * TypeScript shapes for the report JSON payloads. These mirror the C# viewmodels in
 * `Html/ViewModels/` (camelCased by the System.Text.Json source generator). Keep them in
 * sync as renderers are converted.
 */

/** `data/main.json` — the dashboard summary. */
export interface MainSummary {
  databaseName: string;
  databaseVersion: string;
  columnsCount: number;
  constraintsCount: number;
  indexesCount: number;
  schemas: SchemaSummary[];
  schemasCount: number;
  tablesCount: number;
  viewsCount: number;
  sequencesCount: number;
  synonymsCount: number;
  routinesCount: number;
}

/** A schema declared by the database, as listed on the dashboard. */
export interface SchemaSummary {
  name: string;
  /** Whether unqualified object names resolve to this schema. */
  isDefault: boolean;
  /** Whether the database declares this schema itself, e.g. `sys` or `pg_catalog`. */
  isSystem: boolean;
  /** Tables, views, sequences, synonyms and routines the report holds for this schema. */
  objectCount: number;
}

/** A row in `data/tables.json`. */
export interface TableSummary {
  name: string;
  /** Hash route, e.g. `#/tables/actor-d4592e62`. */
  tableUrl: string;
  parentsCount: number;
  childrenCount: number;
  columnCount: number;
  /** Display name of the table kind, e.g. `History`. Empty for an ordinary table. */
  kind: string;
  /**
   * Rows the database reports for the table, almost always an estimate. Null when the report was
   * generated without table statistics, or the database records none for this table.
   */
  rowCount?: number | null;
}

/** `data/tables.json`. */
export interface TablesSummary {
  tablesCount: number;
  allTables: TableSummary[];
}

export interface ParentKey {
  constraintDescription: string;
  parentTableName: string;
  parentTableUrl: string;
  parentColumnName: string;
}

export interface ChildKey {
  constraintDescription: string;
  childTableName: string;
  childTableUrl: string;
  childColumnName: string;
}

export interface TableColumn {
  ordinal: number;
  columnName: string;
  isNullable: boolean;
  type: string;
  defaultValue: string;
  isPrimaryKey: boolean;
  isUniqueKey: boolean;
  isForeignKey: boolean;
  parentKeys: ParentKey[];
  parentKeysCount: number;
  childKeys: ChildKey[];
  childKeysCount: number;
  isAutoIncrement: boolean;
  /** Empty when the database does not report a generation strategy. */
  identityGeneration: string;
  /** Empty when no named sequence backs the column. */
  identitySequenceName: string;
  isComputed: boolean;
  /** Empty when the database does not report the expression. */
  computedDefinition: string;
  /** `Stored`, `Virtual`, or empty when the database does not report how the values are kept. */
  computedStorage: string;
  /** Whether the column is left out of the expansion of `SELECT *`. */
  isHidden: boolean;
}

export interface KeyConstraint {
  constraintName: string;
  columnNames: string;
  /** Whether the database has verified the existing rows against the constraint. */
  isValidated: boolean;
  /** Display name of the deferrability, e.g. `DEFERRABLE INITIALLY DEFERRED`. Empty when the constraint cannot be deferred. */
  deferrabilityDescription: string;
}

export interface ForeignKeyConstraint {
  constraintName: string;
  parentConstraintName: string;
  childColumnNames: string;
  parentTableName: string;
  parentTableUrl: string;
  parentColumnNames: string;
  deleteActionDescription: string;
  updateActionDescription: string;
  /** Whether the database has verified the existing rows against the constraint. */
  isValidated: boolean;
  /** Display name of the deferrability, e.g. `DEFERRABLE INITIALLY DEFERRED`. Empty when the constraint cannot be deferred. */
  deferrabilityDescription: string;
  /** Display name of the match type, e.g. `MATCH FULL`. Empty for the default behaviour. */
  matchTypeDescription: string;
}

export interface CheckConstraint {
  constraintName: string;
  definition: string;
  /** Whether the database has verified the existing rows against the constraint. */
  isValidated: boolean;
  /** Display name of the deferrability, e.g. `DEFERRABLE INITIALLY DEFERRED`. Empty when the constraint cannot be deferred. */
  deferrabilityDescription: string;
}

export interface TableIndex {
  name: string;
  isUnique: boolean;
  columnsText: string;
  includedColumnsText: string;
  /** Display name of the index structure, e.g. `B-Tree`. Empty when the database reports none. */
  indexType: string;
  /** The expression restricting a filtered index to a subset of rows. Empty when unfiltered. */
  filterText: string;
  isEnabled: boolean;
  isValid: boolean;
  isVisible: boolean;
}

export interface TableTrigger {
  triggerName: string;
  definition: string;
  queryTiming: string;
  events: string;
  /** How often the trigger fires. Empty when the database did not report a granularity. */
  granularity: string;
  /** The trigger's `WHEN` clause. Empty when the trigger is unconditional. */
  condition: string;
  /** The trigger's `UPDATE OF` column list. Empty when updates to any column fire it. */
  updateColumns: string;
}

/** A column row shown inside a table node; carries key flags so the UI can filter to a compact view. */
export interface GraphColumn {
  name: string;
  type: string;
  isNullable: boolean;
  isPrimaryKey: boolean;
  isUniqueKey: boolean;
  isForeignKey: boolean;
  /** True when the column participates in any key — the "compact" view filter. */
  isKey: boolean;
}

/** A table node in a relationship diagram. `id` is the table's safe key (its SPA route param). */
export interface GraphTable {
  id: string;
  name: string;
  /** Hash route to the table's detail page, e.g. `#/tables/<safeKey>`. */
  tableUrl: string;
  columns: GraphColumn[];
  columnsCount: number;
  parentKeysCount: number;
  childKeysCount: number;
  /** The focal table of a per-table diagram; drawn with the highlight palette. */
  isHighlighted: boolean;
}

/** A directed foreign-key edge, pointing from the child (referencing) table to the parent. */
export interface GraphEdge {
  id: string;
  childTableId: string;
  parentTableId: string;
  constraintName: string;
  childColumns: string[];
  parentColumns: string[];
}

/** A relationship diagram as plain data — table nodes and the foreign-key edges between them. */
export interface RelationshipGraph {
  nodes: GraphTable[];
  nodesCount: number;
  edges: GraphEdge[];
  edgesCount: number;
}

export interface TableDiagram {
  name: string;
  containerId: string;
  isActive: boolean;
  graph: RelationshipGraph;
}

/** A row in `data/views.json`. */
export interface ViewSummary {
  name: string;
  /** Hash route, e.g. `#/views/<safeKey>`. */
  viewUrl: string;
  columnCount: number;
  isMaterialized: boolean;
}

/** `data/views.json`. */
export interface ViewsSummary {
  viewsCount: number;
  allViews: ViewSummary[];
}

export interface ViewColumn {
  ordinal: number;
  columnName: string;
  isNullable: boolean;
  type: string;
  defaultValue: string;
}

/** A link from a view to an object it references (hash route into the SPA). */
export interface ReferencedObject {
  name: string;
  url: string;
}

/** `data/views/<safeKey>.json`. */
export interface ViewDetail {
  name: string;
  viewUrl: string;
  definition: string;
  columns: ViewColumn[];
  columnsCount: number;
  referencedObjects: ReferencedObject[];
  referencedObjectsCount: number;
  indexes: TableIndex[];
  indexesCount: number;
  triggers: TableTrigger[];
  triggersCount: number;
  /** Display name of the check option, e.g. `WITH CASCADED CHECK OPTION`. Empty when the view has none. */
  checkOption: string;
  isUpdatable: boolean;
  isMaterialized: boolean;
  /** Display name of the refresh mode, e.g. `ON DEMAND`. Empty when the view is not materialized, or the database reported none. */
  refreshMode: string;
  /** How a materialized view is refreshed, e.g. `FAST`. Empty when the database has only one refresh method. */
  refreshMethod: string;
  isPopulated: boolean;
}

/** A row in `data/routines.json`. */
export interface RoutineSummary {
  name: string;
  /** Hash route, e.g. `#/routines/<safeKey>`. */
  routineUrl: string;
  /** The kind of routine, e.g. `Procedure`; `Unknown` when the database records no kind. */
  routineType: RoutineType;
}

/** `data/routines.json`. */
export interface RoutinesSummary {
  routinesCount: number;
  allRoutines: RoutineSummary[];
}

/** The kind of routine a database object represents. */
export type RoutineType = "Unknown" | "Procedure" | "Function" | "Package" | "Aggregate";

/** How a value flows through a routine parameter. */
export type RoutineParameterDirection = "Input" | "Output" | "InputOutput";

/** One parameter of a routine's signature. */
export interface RoutineParameter {
  /** Omitted from the JSON when the parameter is positional. */
  parameterName?: string;
  type: string;
  direction: RoutineParameterDirection;
  /** Omitted from the JSON when the parameter has no default. */
  defaultValue?: string;
  ordinal: number;
}

/** One signature of a routine whose name carries more than one. */
export interface RoutineOverload {
  definition: string;
  parameters: RoutineParameter[];
  /** Omitted from the JSON when the signature returns nothing. */
  returnType?: string;
}

/** `data/routines/<safeKey>.json`. */
export interface RoutineDetail {
  name: string;
  routineUrl: string;
  definition: string;
  routineType: RoutineType;
  /** Omitted from the JSON when the database records no language. */
  language?: string;
  parameters: RoutineParameter[];
  parametersCount: number;
  /** Omitted from the JSON when the routine returns nothing. */
  returnType?: string;
  /** Empty unless the routine's name carries more than one signature. */
  overloads: RoutineOverload[];
  overloadsCount: number;
}

/** A row in `data/sequences.json`; also the per-sequence detail (`data/sequences/<safeKey>.json`). */
export interface SequenceSummary {
  name: string;
  /** Hash route, e.g. `#/sequences/<safeKey>`. */
  sequenceUrl: string;
  /** The declared type of the generated values, e.g. `bigint`. */
  type: string;
  start: number;
  increment: number;
  /** Omitted from the JSON when the sequence has no minimum. */
  minValue?: number;
  /** Omitted from the JSON when the sequence has no maximum. */
  maxValue?: number;
  /** A cache size, `None`, `Database default`, or empty when the database reports nothing. */
  cache: string;
  cycle: boolean;
  /** Only Oracle distinguishes an unordered sequence; elsewhere this is always true. */
  isOrdered: boolean;
}

/** `data/sequences.json`. */
export interface SequencesSummary {
  sequencesCount: number;
  allSequences: SequenceSummary[];
}

/** `data/sequences/<safeKey>.json`. Structurally identical to a summary row. */
export type SequenceDetail = SequenceSummary;

/** A row in `data/synonyms.json`; also the per-synonym detail (`data/synonyms/<safeKey>.json`). */
export interface SynonymSummary {
  name: string;
  /** Hash route, e.g. `#/synonyms/<safeKey>`. */
  synonymUrl: string;
  targetName: string;
  /** Target's hash route; omitted from the JSON when the target is not a known object. */
  targetUrl?: string;
}

/** `data/synonyms.json`. */
export interface SynonymsSummary {
  synonymsCount: number;
  allSynonyms: SynonymSummary[];
}

/** `data/synonyms/<safeKey>.json`. Structurally identical to a summary row. */
export type SynonymDetail = SynonymSummary;

/** A row in `data/triggers.json`. */
export interface TriggerRow {
  name: string;
  /** The table or view the trigger is defined on. */
  objectName: string;
  /** Hash route to the owning table or view. */
  objectUrl: string;
  definition: string;
  queryTiming: string;
  events: string;
  /** How often the trigger fires. Empty when the database did not report a granularity. */
  granularity: string;
  /** The trigger's `WHEN` clause. Empty when the trigger is unconditional. */
  condition: string;
  /** The trigger's `UPDATE OF` column list. Empty when updates to any column fire it. */
  updateColumns: string;
}

/** `data/triggers.json`. */
export interface TriggersSummary {
  triggersCount: number;
  allTriggers: TriggerRow[];
}

export type ColumnParentType = "Table" | "View";

/** A row in `data/columns.json` (a column of a table or view). */
export interface ColumnRow {
  /** Parent table/view name. */
  name: string;
  parentType: ColumnParentType;
  /** Hash route to the parent table/view. */
  parentUrl: string;
  ordinal: number;
  columnName: string;
  type: string;
  isNullable: boolean;
  defaultValue: string;
  isPrimaryKey: boolean;
  isUniqueKey: boolean;
  isForeignKey: boolean;
}

/** `data/columns.json`. */
export interface ColumnsSummary {
  columnsCount: number;
  tableColumns: ColumnRow[];
}

/** Fields shared by every constraint row in `data/constraints.json`. */
interface ConstraintBase {
  tableName: string;
  /** Hash route to the owning table. */
  tableUrl: string;
  constraintName: string;
  /** Whether the database has verified the existing rows against the constraint. */
  isValidated: boolean;
  /** Display name of the deferrability, e.g. `DEFERRABLE INITIALLY DEFERRED`. Empty when the constraint cannot be deferred. */
  deferrabilityDescription: string;
}

export interface PrimaryKeyConstraintRow extends ConstraintBase {
  columnNames: string;
}

export interface UniqueKeyRow extends ConstraintBase {
  columnNames: string;
}

export interface ForeignKeyRow extends ConstraintBase {
  childColumnNames: string;
  parentConstraintName: string;
  parentTableName: string;
  /** Hash route to the referenced (parent) table. */
  parentTableUrl: string;
  parentColumnNames: string;
  deleteActionDescription: string;
  updateActionDescription: string;
  /** Display name of the match type, e.g. `MATCH FULL`. Empty for the default behaviour. */
  matchTypeDescription: string;
}

export interface CheckConstraintRow extends ConstraintBase {
  definition: string;
}

/** `data/constraints.json`. */
export interface ConstraintsSummary {
  primaryKeys: PrimaryKeyConstraintRow[];
  primaryKeysCount: number;
  uniqueKeys: UniqueKeyRow[];
  uniqueKeysCount: number;
  foreignKeys: ForeignKeyRow[];
  foreignKeysCount: number;
  checkConstraints: CheckConstraintRow[];
  checkConstraintsCount: number;
}

/** A row in `data/indexes.json`. */
export interface IndexRow {
  name: string;
  tableName: string;
  /** Hash route to the owning table. */
  tableUrl: string;
  isUnique: boolean;
  columnsText: string;
  includedColumnsText: string;
  /** Display name of the index structure, e.g. `B-Tree`. Empty when the database reports none. */
  indexType: string;
  /** The expression restricting a filtered index to a subset of rows. Empty when unfiltered. */
  filterText: string;
  isEnabled: boolean;
  isValid: boolean;
  isVisible: boolean;
}

/** `data/indexes.json`. */
export interface IndexesSummary {
  indexesCount: number;
  tableIndexes: IndexRow[];
}

/** A row in `data/orphans.json` (a table with no relationships). */
export interface OrphanTable {
  name: string;
  tableUrl: string;
  columnCount: number;
}

/** `data/orphans.json`. */
export interface OrphansSummary {
  tablesCount: number;
  tables: OrphanTable[];
}

/** Severity of a lint finding, ordered least to most severe. */
export type LintLevel = "Information" | "Warning" | "Error";

/** The kind of database object a lint message is attributed to. */
export type LintObjectType = "Table" | "View" | "Sequence" | "Synonym" | "Routine";

/** A rule that raised at least one message, in `data/lint.json`. */
export interface LintRule {
  /** Stable rule identifier, e.g. `SCHEMATIC0009`. */
  ruleId: string;
  ruleTitle: string;
  level: LintLevel;
  messageCount: number;
}

/**
 * A single lint finding. Messages are a flat list rather than being nested inside their rule;
 * `ruleId` joins a message back to its entry in `lintRules`.
 */
export interface LintMessage {
  ruleId: string;
  ruleTitle: string;
  level: LintLevel;
  message: string;
  /** Qualified name of the object this concerns; absent for schema-wide findings. */
  objectName?: string;
  objectType?: LintObjectType;
  /** Hash route to the object's detail page, e.g. `#/tables/actor-d4592e62`. */
  objectUrl?: string;
}

/** `data/lint.json`. */
export interface LintSummary {
  /** How many distinct rules fired. Not the number of issues — that is `messageCount`. */
  lintRulesCount: number;
  lintRules: LintRule[];
  messageCount: number;
  messages: LintMessage[];
  errorCount: number;
  warningCount: number;
  informationCount: number;
  objectsAffectedCount: number;
}

/** `data/relationships.json` — the schema-wide relationship graph (laid out and drawn client-side). */
export interface RelationshipsSummary {
  graph: RelationshipGraph;
}

/** A single entry in `data/search.json`. */
export interface SearchEntry {
  name: string;
  objectType: string;
  /** In-app hash route, e.g. `#/tables/<safeKey>`. */
  url: string;
  /** Owning object's name for column entries; omitted for top-level objects. */
  parent?: string;
}

/** `data/search.json`. */
export interface SearchSummary {
  entriesCount: number;
  entries: SearchEntry[];
}

/** `data/tables/<safeKey>.json`. */
/** A table named by another table's storage metadata. */
export interface LinkedTable {
  name: string;
  /** Hash route into the SPA. Empty when the report has no page for the object. */
  tableUrl: string;
}

/** How a table's rows are distributed across partitions. */
export interface TablePartitioning {
  /** How rows are assigned to a partition, e.g. `RANGE`. */
  strategy: string;
  /** The columns the partitioning key is built from. Empty when the database does not report them. */
  columnNames: string[];
  partitions: LinkedTable[];
  partitionsCount: number;
}

/** Where a table's superseded rows are retained. */
export interface TableSystemVersioning {
  historyTable: LinkedTable;
  periodStartColumn: string;
  periodEndColumn: string;
}

export interface TableDetail {
  name: string;
  tableUrl: string;
  columns: TableColumn[];
  columnsCount: number;
  /** Omitted from the JSON when the table has no primary key. */
  primaryKey?: KeyConstraint;
  primaryKeyExists: boolean;
  uniqueKeys: KeyConstraint[];
  uniqueKeysCount: number;
  foreignKeys: ForeignKeyConstraint[];
  foreignKeysCount: number;
  checkConstraints: CheckConstraint[];
  checkConstraintsCount: number;
  indexes: TableIndex[];
  indexesCount: number;
  triggers: TableTrigger[];
  triggersCount: number;
  diagrams: TableDiagram[];
  /** Display name of the table kind, e.g. `History`. Empty for an ordinary table. */
  kind: string;
  /** Omitted from the JSON when the table is not partitioned. */
  tablePartitioning?: TablePartitioning;
  /** Omitted from the JSON when the table is not system-versioned. */
  tableSystemVersioning?: TableSystemVersioning;
  isLogged: boolean;
  /** The table's default collation. Empty when the database records none for the table as a whole. */
  collation: string;
}
