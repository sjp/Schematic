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
