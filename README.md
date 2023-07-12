# DatingApp

https://github.com/TryCatchLearn/DatingApp - Original project repository

```typescript
	deadlinesColumns: ColumnItem[] = [
		{
			name: 'Tip Termen',
			i18name: 'app.applications.table.columns.termTypeName',
			key: 'termTypeName',
			sortOrder: null
		},
		{
		name: 'Publicare',
		i18name: 'app.applications.table.columns.isPublished',
		key: 'isPublished',
		sortOrder: null,
		rendering: (isPublised: number) => !!isPublised ? 'Da' : 'Nu'
		},
		{
		name: 'Dată',
		i18name: 'app.applications.table.columns.termDate',
		key: 'termDate',
		sortOrder: null,
		type: ColumnType.DATE
		},
		{
		name: 'Oră',
		i18name: 'app.applications.table.columns.termDate',
		key: 'termDate',
		sortOrder: null,
		type: ColumnType.HOUR
		},
	];
```
