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
  schemas: string[];
  schemasCount: number;
  tablesCount: number;
  viewsCount: number;
  sequencesCount: number;
  synonymsCount: number;
  routinesCount: number;
}

/** A row in `data/tables.json`. */
export interface TableSummary {
  name: string;
  /** Hash route, e.g. `#/tables/actor-d4592e62`. */
  tableUrl: string;
  parentsCount: number;
  childrenCount: number;
  columnCount: number;
  rowCount: number;
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
}

export interface KeyConstraint {
  constraintName: string;
  columnNames: string;
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
}

export interface CheckConstraint {
  constraintName: string;
  definition: string;
}

export interface TableIndex {
  name: string;
  isUnique: boolean;
  columnsText: string;
  includedColumnsText: string;
}

export interface TableTrigger {
  triggerName: string;
  definition: string;
  queryTiming: string;
  events: string;
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
  rowCount: number;
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
}

/** A row in `data/routines.json`. */
export interface RoutineSummary {
  name: string;
  /** Hash route, e.g. `#/routines/<safeKey>`. */
  routineUrl: string;
}

/** `data/routines.json`. */
export interface RoutinesSummary {
  routinesCount: number;
  allRoutines: RoutineSummary[];
}

/** `data/routines/<safeKey>.json`. */
export interface RoutineDetail {
  name: string;
  routineUrl: string;
  definition: string;
}

/** A row in `data/sequences.json`; also the per-sequence detail (`data/sequences/<safeKey>.json`). */
export interface SequenceSummary {
  name: string;
  /** Hash route, e.g. `#/sequences/<safeKey>`. */
  sequenceUrl: string;
  start: number;
  increment: number;
  /** Omitted from the JSON when the sequence has no minimum. */
  minValue?: number;
  /** Omitted from the JSON when the sequence has no maximum. */
  maxValue?: number;
  cache: number;
  cycle: boolean;
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
  tableName: string;
  /** Hash route to the owning table. */
  tableUrl: string;
  definition: string;
  queryTiming: string;
  events: string;
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
  rowCount: number;
}

/** `data/orphans.json`. */
export interface OrphansSummary {
  tablesCount: number;
  tables: OrphanTable[];
}

/** A group of lint messages produced by a single rule, in `data/lint.json`. */
export interface LintRule {
  ruleTitle: string;
  messages: string[];
  messageCount: number;
}

/** `data/lint.json`. */
export interface LintSummary {
  lintRulesCount: number;
  lintRules: LintRule[];
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
export interface TableDetail {
  name: string;
  tableUrl: string;
  rowCount: number;
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
}
