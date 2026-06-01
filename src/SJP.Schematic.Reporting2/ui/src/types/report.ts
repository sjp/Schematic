/**
 * TypeScript shapes for the report JSON payloads. These mirror the C# viewmodels in
 * `Html/ViewModels/` (camelCased by the System.Text.Json source generator). Keep them in
 * sync as renderers are converted.
 */

/** `data/main.json` — the dashboard summary. */
export interface MainSummary {
  databaseName: string
  databaseVersion: string
  columnsCount: number
  constraintsCount: number
  indexesCount: number
  schemas: string[]
  schemasCount: number
  tablesCount: number
  viewsCount: number
  sequencesCount: number
  synonymsCount: number
  routinesCount: number
}

/** A row in `data/tables.json`. */
export interface TableSummary {
  name: string
  /** Hash route, e.g. `#/tables/actor-d4592e62`. */
  tableUrl: string
  parentsCount: number
  childrenCount: number
  columnCount: number
  rowCount: number
}

/** `data/tables.json`. */
export interface TablesSummary {
  tablesCount: number
  allTables: TableSummary[]
}

export interface ParentKey {
  constraintDescription: string
  parentTableName: string
  parentTableUrl: string
  parentColumnName: string
}

export interface ChildKey {
  constraintDescription: string
  childTableName: string
  childTableUrl: string
  childColumnName: string
}

export interface TableColumn {
  ordinal: number
  columnName: string
  isNullable: boolean
  type: string
  defaultValue: string
  isPrimaryKey: boolean
  isUniqueKey: boolean
  isForeignKey: boolean
  parentKeys: ParentKey[]
  parentKeysCount: number
  childKeys: ChildKey[]
  childKeysCount: number
}

export interface KeyConstraint {
  constraintName: string
  columnNames: string
}

export interface ForeignKeyConstraint {
  constraintName: string
  parentConstraintName: string
  childColumnNames: string
  parentTableName: string
  parentTableUrl: string
  parentColumnNames: string
  deleteActionDescription: string
  updateActionDescription: string
}

export interface CheckConstraint {
  constraintName: string
  definition: string
}

export interface TableIndex {
  name: string
  isUnique: boolean
  columnsText: string
  includedColumnsText: string
}

export interface TableTrigger {
  triggerName: string
  definition: string
  queryTiming: string
  events: string
}

export interface TableDiagram {
  name: string
  containerId: string
  isActive: boolean
  /** Path relative to the report root, e.g. `data/diagrams/<id>.svg`. */
  svgFile: string
}

/** A row in `data/views.json`. */
export interface ViewSummary {
  name: string
  /** Hash route, e.g. `#/views/<safeKey>`. */
  viewUrl: string
  columnCount: number
  isMaterialized: boolean
}

/** `data/views.json`. */
export interface ViewsSummary {
  viewsCount: number
  allViews: ViewSummary[]
}

export interface ViewColumn {
  ordinal: number
  columnName: string
  isNullable: boolean
  type: string
  defaultValue: string
}

/** A link from a view to an object it references (hash route into the SPA). */
export interface ReferencedObject {
  name: string
  url: string
}

/** `data/views/<safeKey>.json`. */
export interface ViewDetail {
  name: string
  viewUrl: string
  definition: string
  columns: ViewColumn[]
  columnsCount: number
  referencedObjects: ReferencedObject[]
  referencedObjectsCount: number
}

/** A row in `data/routines.json`. */
export interface RoutineSummary {
  name: string
  /** Hash route, e.g. `#/routines/<safeKey>`. */
  routineUrl: string
}

/** `data/routines.json`. */
export interface RoutinesSummary {
  routinesCount: number
  allRoutines: RoutineSummary[]
}

/** `data/routines/<safeKey>.json`. */
export interface RoutineDetail {
  name: string
  routineUrl: string
  definition: string
}

/** A row in `data/sequences.json`; also the per-sequence detail (`data/sequences/<safeKey>.json`). */
export interface SequenceSummary {
  name: string
  /** Hash route, e.g. `#/sequences/<safeKey>`. */
  sequenceUrl: string
  start: number
  increment: number
  /** Omitted from the JSON when the sequence has no minimum. */
  minValue?: number
  /** Omitted from the JSON when the sequence has no maximum. */
  maxValue?: number
  cache: number
  cycle: boolean
}

/** `data/sequences.json`. */
export interface SequencesSummary {
  sequencesCount: number
  allSequences: SequenceSummary[]
}

/** `data/sequences/<safeKey>.json`. Structurally identical to a summary row. */
export type SequenceDetail = SequenceSummary

/** A row in `data/synonyms.json`; also the per-synonym detail (`data/synonyms/<safeKey>.json`). */
export interface SynonymSummary {
  name: string
  /** Hash route, e.g. `#/synonyms/<safeKey>`. */
  synonymUrl: string
  targetName: string
  /** Target's hash route; omitted from the JSON when the target is not a known object. */
  targetUrl?: string
}

/** `data/synonyms.json`. */
export interface SynonymsSummary {
  synonymsCount: number
  allSynonyms: SynonymSummary[]
}

/** `data/synonyms/<safeKey>.json`. Structurally identical to a summary row. */
export type SynonymDetail = SynonymSummary

/** `data/tables/<safeKey>.json`. */
export interface TableDetail {
  name: string
  tableUrl: string
  rowCount: number
  columns: TableColumn[]
  columnsCount: number
  /** Omitted from the JSON when the table has no primary key. */
  primaryKey?: KeyConstraint
  primaryKeyExists: boolean
  uniqueKeys: KeyConstraint[]
  uniqueKeysCount: number
  foreignKeys: ForeignKeyConstraint[]
  foreignKeysCount: number
  checkConstraints: CheckConstraint[]
  checkConstraintsCount: number
  indexes: TableIndex[]
  indexesCount: number
  triggers: TableTrigger[]
  triggersCount: number
  diagrams: TableDiagram[]
}
