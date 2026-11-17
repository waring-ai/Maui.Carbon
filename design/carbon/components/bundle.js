/* @ds-bundle: {"format":4,"namespace":"Carbon","components":[{"name":"Button"},{"name":"Tag"},{"name":"TextInput"},{"name":"Checkbox"},{"name":"Toggle"},{"name":"InlineNotification"},{"name":"Tile"},{"name":"Link"},{"name":"Tabs"},{"name":"Icon"},{"name":"TextArea"},{"name":"NumberInput"},{"name":"PasswordInput"},{"name":"Search"},{"name":"Select"},{"name":"RadioButtonGroup"},{"name":"TimePicker"},{"name":"Slider"},{"name":"Dropdown"},{"name":"ComboBox"},{"name":"MultiSelect"},{"name":"FileUploader"},{"name":"DatePicker"},{"name":"DataTable"},{"name":"Pagination"},{"name":"PaginationNav"},{"name":"StructuredList"},{"name":"ContainedList"},{"name":"Accordion"},{"name":"TreeView"},{"name":"CodeSnippet"},{"name":"ProgressBar"},{"name":"ProgressIndicator"},{"name":"SkeletonText"},{"name":"SkeletonPlaceholder"},{"name":"Loading"},{"name":"InlineLoading"},{"name":"Breadcrumb"},{"name":"ContentSwitcher"},{"name":"Menu"},{"name":"MenuButton"},{"name":"OverflowMenu"},{"name":"ComboButton"},{"name":"Modal"},{"name":"ToastNotification"},{"name":"ActionableNotification"},{"name":"Popover"},{"name":"Tooltip"},{"name":"Toggletip"},{"name":"IconButton"},{"name":"CopyButton"},{"name":"IconIndicator"},{"name":"ShapeIndicator"},{"name":"BadgeIndicator"},{"name":"Layer"},{"name":"Stack"},{"name":"Grid"},{"name":"ButtonSet"},{"name":"AspectRatio"},{"name":"SelectableTile"},{"name":"ExpandableTile"},{"name":"RadioTile"},{"name":"OrderedList"},{"name":"AILabel"},{"name":"ChatButton"},{"name":"FluidForm"},{"name":"FluidTextInput"},{"name":"FluidTextArea"},{"name":"FluidNumberInput"},{"name":"FluidSearch"},{"name":"FluidSelect"},{"name":"FluidTimePicker"},{"name":"FluidDropdown"},{"name":"FluidComboBox"},{"name":"FluidMultiSelect"},{"name":"FluidDatePicker"},{"name":"Form"},{"name":"CheckboxGroup"},{"name":"Heading"},{"name":"Text"},{"name":"Theme"},{"name":"DismissibleTag"},{"name":"OperationalTag"},{"name":"SelectableTag"},{"name":"Callout"},{"name":"DataTableSkeleton"},{"name":"SkeletonIcon"},{"name":"DefinitionTooltip"},{"name":"PageHeader"},{"name":"Dialog"},{"name":"HeaderPanel"}]} */
(function () {
  var React = window.React;
  var h = React.createElement;
  var useState = React.useState;
  var useId = React.useId;
  var useRef = React.useRef;
  var useEffect = React.useEffect;
  var useLayoutEffect = React.useLayoutEffect;

  var ICONS = {"arrow--right-16":{"vb":"0 0 16 16","body":"<polygon points=\"9.3,3.7 13.1,7.5 1,7.5 1,8.5 13.1,8.5 9.3,12.3 10,13 15,8 10,3 \"/>"},"arrow--up-16":{"vb":"0 0 16 16","body":"<polygon points=\"3.7,6.7 7.5,2.9 7.5,15 8.5,15 8.5,2.9 12.3,6.7 13,6 8,1 3,6 \"/>"},"checkmark--filled-16":{"vb":"0 0 16 16","body":"<path d=\"M8,1C4.1,1,1,4.1,1,8c0,3.9,3.1,7,7,7s7-3.1,7-7C15,4.1,11.9,1,8,1z M7,11L4.3,8.3l0.9-0.8L7,9.3l4-3.9l0.9,0.8L7,11z\"/>"},"chevron--down-16":{"vb":"0 0 16 16","body":"<polygon points=\"8,11 3,6 3.7,5.3 8,9.6 12.3,5.3 13,6 \"/>"},"chevron--left-16":{"vb":"0 0 16 16","body":"<polygon points=\"5,8 10,3 10.7,3.7 6.4,8 10.7,12.3 10,13 \"/>"},"chevron--right-16":{"vb":"0 0 16 16","body":"<polygon points=\"11,8 6,13 5.3,12.3 9.6,8 5.3,3.7 6,3 \"/>"},"chevron--up-16":{"vb":"0 0 16 16","body":"<polygon points=\"8,5 13,10 12.3,10.7 8,6.4 3.7,10.7 3,10 \"/>"},"close--filled-16":{"vb":"0 0 16 16","body":"<path d=\"M8,1C4.1,1,1,4.1,1,8s3.1,7,7,7s7-3.1,7-7S11.9,1,8,1z M10.7,11.5L8,8.8l-2.7,2.7l-0.8-0.8L7.2,8L4.5,5.3l0.8-0.8L8,7.2 l2.7-2.7l0.8,0.8L8.8,8l2.7,2.7L10.7,11.5z\"/>"},"download-16":{"vb":"0 0 16 16","body":"<polygon points=\"13,7 12.3,6.3 8.5,10.1 8.5,1 7.5,1 7.5,10.1 3.7,6.3 3,7 8,12 \"/> <path d=\"M13,12v2H3v-2H2v2l0,0c0,0.6,0.4,1,1,1h10c0.6,0,1-0.4,1-1l0,0v-2H13z\"/>"},"error--filled-16":{"vb":"0 0 16 16","body":"<path d=\"M8,1C4.1,1,1,4.1,1,8s3.1,7,7,7s7-3.1,7-7S11.9,1,8,1z M10.7,11.5L4.5,5.3l0.8-0.8l6.2,6.2L10.7,11.5z\"/>"},"export-16":{"vb":"0 0 16 16","body":"<path d=\"M13,12v2H3v-2H2v2l0,0c0,0.6,0.4,1,1,1h10c0.6,0,1-0.4,1-1l0,0v-2H13z\"/> <polygon points=\"3,6 3.7,6.7 7.5,2.9 7.5,12 8.5,12 8.5,2.9 12.3,6.7 13,6 8,1 \"/>"},"help-16":{"vb":"0 0 16 16","body":"<path d=\"M8,1C4.1,1,1,4.1,1,8s3.1,7,7,7s7-3.1,7-7S11.9,1,8,1z M8,14c-3.3,0-6-2.7-6-6s2.7-6,6-6s6,2.7,6,6S11.3,14,8,14z\"/> <circle cx=\"8\" cy=\"11.8\" r=\"0.8\"/> <path d=\"M8.5,4H7.8C6.5,4,5.5,5,5.5,6.2c0,0,0,0,0,0v0.2h1V6.2C6.5,5.6,7.1,5,7.8,5h0.8c0.7,0,1.2,0.6,1.2,1.2S9.2,7.5,8.5,7.5h-1 v2.2h1V8.5c1.2,0,2.2-1,2.2-2.2S9.7,4,8.5,4z\"/>"},"information-16":{"vb":"0 0 16 16","body":"<polygon points=\"8.5,11 8.5,6.5 6.5,6.5 6.5,7.5 7.5,7.5 7.5,11 6,11 6,12 10,12 10,11 \"/> <path d=\"M8,3.5c-0.4,0-0.8,0.3-0.8,0.8S7.6,5,8,5c0.4,0,0.8-0.3,0.8-0.8S8.4,3.5,8,3.5z\"/> <path d=\"M8,15c-3.9,0-7-3.1-7-7s3.1-7,7-7s7,3.1,7,7S11.9,15,8,15z M8,2C4.7,2,2,4.7,2,8s2.7,6,6,6s6-2.7,6-6S11.3,2,8,2z\"/>"},"launch-16":{"vb":"0 0 16 16","body":"<path d=\"M13,14H3c-0.6,0-1-0.4-1-1V3c0-0.6,0.4-1,1-1h5v1H3v10h10V8h1v5C14,13.6,13.6,14,13,14z\"/> <polygon points=\"10,1 10,2 13.3,2 9,6.3 9.7,7 14,2.7 14,6 15,6 15,1 \"/>"},"menu-16":{"vb":"0 0 16 16","body":"<rect x=\"2\" y=\"12\" width=\"12\" height=\"1\"/> <rect x=\"2\" y=\"9\" width=\"12\" height=\"1\"/> <rect x=\"2\" y=\"6\" width=\"12\" height=\"1\"/> <rect x=\"2\" y=\"3\" width=\"12\" height=\"1\"/>"},"notification-16":{"vb":"0 0 16 16","body":"<path d=\"M14.4,10.1L13,8.8V6.5c0-2.6-1.9-4.7-4.5-5v-1h-1v1C5,1.8,3,3.9,3,6.5v2.3l-1.4,1.3c-0.1,0.1-0.2,0.2-0.1,0.4V12 c0,0.3,0.2,0.5,0.4,0.5c0,0,0,0,0.1,0h3.5C5.5,13.9,6.6,15,8,15s2.5-1.1,2.5-2.5H14c0.3,0,0.5-0.2,0.5-0.4c0,0,0,0,0-0.1v-1.5 C14.5,10.4,14.4,10.2,14.4,10.1z M8,14c-0.8,0-1.5-0.7-1.5-1.5h3C9.5,13.3,8.8,14,8,14z M13.5,11.5h-11v-0.8l1.3-1.4 C3.9,9.3,4,9.1,4,9V6.5c0-2.2,1.8-4,4-4s4,1.8,4,4V9c0,0.1,0.1,0.3,0.1,0.4l1.4,1.3V11.5z\"/>"},"play-16":{"vb":"0 0 16 16","body":"<path d=\"M3.5,14C3.2,14,3,13.8,3,13.5v-11c0-0.2,0.1-0.3,0.2-0.4C3.4,2,3.6,2,3.8,2.1l9.5,5.5c0.2,0.1,0.3,0.4,0.2,0.7 c0,0.1-0.1,0.1-0.2,0.2l-9.5,5.5C3.7,14,3.6,14,3.5,14z M4,3.4v9.3L12,8L4,3.4z\"/>"},"restart-16":{"vb":"0 0 16 16","body":"<path d=\"M13,9c0,2.8-2.2,5-5,5s-5-2.2-5-5s2.2-5,5-5h3.1L9.3,5.8L10,6.5l3-3l-3-3L9.3,1.2L11.1,3H8C4.7,3,2,5.7,2,9s2.7,6,6,6 s6-2.7,6-6H13z\"/>"},"save-16":{"vb":"0 0 16 16","body":"<path d=\"M13.9,4.6l-2.5-2.5C11.3,2.1,11.1,2,11,2H3C2.4,2,2,2.4,2,3v10c0,0.6,0.4,1,1,1h10c0.6,0,1-0.4,1-1V5 C14,4.9,13.9,4.7,13.9,4.6z M6,3h4v2H6V3z M10,13H6V9h4V13z M11,13V9c0-0.6-0.4-1-1-1H6C5.4,8,5,8.4,5,9v4H3V3h2v2c0,0.6,0.4,1,1,1 h4c0.6,0,1-0.4,1-1V3.2l2,2V13H11z\"/>"},"search-16":{"vb":"0 0 16 16","body":"<path d=\"M15,14.3L10.7,10c1.9-2.3,1.6-5.8-0.7-7.7S4.2,0.7,2.3,3S0.7,8.8,3,10.7c2,1.7,5,1.7,7,0l4.3,4.3L15,14.3z M2,6.5 C2,4,4,2,6.5,2S11,4,11,6.5S9,11,6.5,11S2,9,2,6.5z\"/>"},"settings-16":{"vb":"0 0 16 16","body":"<path d=\"M13.5,8.4c0-0.1,0-0.3,0-0.4c0-0.1,0-0.3,0-0.4l1-0.8c0.4-0.3,0.4-0.9,0.2-1.3l-1.2-2C13.3,3.2,13,3,12.6,3 c-0.1,0-0.2,0-0.3,0.1l-1.2,0.4c-0.2-0.1-0.4-0.3-0.7-0.4l-0.3-1.3C10.1,1.3,9.7,1,9.2,1H6.8c-0.5,0-0.9,0.3-1,0.8L5.6,3.1 C5.3,3.2,5.1,3.3,4.9,3.4L3.7,3C3.6,3,3.5,3,3.4,3C3,3,2.7,3.2,2.5,3.5l-1.2,2C1.1,5.9,1.2,6.4,1.6,6.8l0.9,0.9c0,0.1,0,0.3,0,0.4 c0,0.1,0,0.3,0,0.4L1.6,9.2c-0.4,0.3-0.5,0.9-0.2,1.3l1.2,2C2.7,12.8,3,13,3.4,13c0.1,0,0.2,0,0.3-0.1l1.2-0.4 c0.2,0.1,0.4,0.3,0.7,0.4l0.3,1.3c0.1,0.5,0.5,0.8,1,0.8h2.4c0.5,0,0.9-0.3,1-0.8l0.3-1.3c0.2-0.1,0.4-0.2,0.7-0.4l1.2,0.4 c0.1,0,0.2,0.1,0.3,0.1c0.4,0,0.7-0.2,0.9-0.5l1.1-2c0.2-0.4,0.2-0.9-0.2-1.3L13.5,8.4z M12.6,12l-1.7-0.6c-0.4,0.3-0.9,0.6-1.4,0.8 L9.2,14H6.8l-0.4-1.8c-0.5-0.2-0.9-0.5-1.4-0.8L3.4,12l-1.2-2l1.4-1.2c-0.1-0.5-0.1-1.1,0-1.6L2.2,6l1.2-2l1.7,0.6 C5.5,4.2,6,4,6.5,3.8L6.8,2h2.4l0.4,1.8c0.5,0.2,0.9,0.5,1.4,0.8L12.6,4l1.2,2l-1.4,1.2c0.1,0.5,0.1,1.1,0,1.6l1.4,1.2L12.6,12z\"/> <path d=\"M8,11c-1.7,0-3-1.3-3-3s1.3-3,3-3s3,1.3,3,3C11,9.6,9.7,11,8,11C8,11,8,11,8,11z M8,6C6.9,6,6,6.8,6,7.9C6,7.9,6,8,6,8 c0,1.1,0.8,2,1.9,2c0,0,0.1,0,0.1,0c1.1,0,2-0.8,2-1.9c0,0,0-0.1,0-0.1C10,6.9,9.2,6,8,6C8.1,6,8,6,8,6z\"/>"},"star-16":{"vb":"0 0 16 16","body":"<path d=\"M8,3.3l1.4,2.8l0.2,0.5l0.5,0.1l3.1,0.4L11,9.2l-0.4,0.4l0.1,0.5l0.5,3.1l-2.8-1.4L8,11.5l-0.5,0.2l-2.8,1.4l0.5-3.1 l0.1-0.5L5,9.2L2.8,7l3.1-0.4l0.5-0.1L6.6,6L8,3.3 M8,1L5.7,5.6L0.6,6.3l3.7,3.6L3.5,15L8,12.6l4.6,2.4l-0.9-5.1l3.7-3.6l-5.1-0.7 L8,1z\"/>"},"stop-16":{"vb":"0 0 16 16","body":"<path d=\"M12,4v8H4V4H12 M12,3H4C3.4,3,3,3.4,3,4v8c0,0.6,0.4,1,1,1h8c0.6,0,1-0.4,1-1V4C13,3.4,12.6,3,12,3z\"/>"},"upload-16":{"vb":"0 0 16 16","body":"<polygon points=\"3,9 3.7,9.7 7.5,5.9 7.5,15 8.5,15 8.5,5.9 12.3,9.7 13,9 8,4 \"/> <path d=\"M3,4V2h10v2h1V2c0-0.6-0.4-1-1-1H3C2.4,1,2,1.4,2,2v2H3z\"/>"},"user-16":{"vb":"0 0 16 16","body":"<path d=\"M8,2c1.4,0,2.5,1.1,2.5,2.5S9.4,7,8,7S5.5,5.9,5.5,4.5S6.6,2,8,2 M8,1C6.1,1,4.5,2.6,4.5,4.5S6.1,8,8,8s3.5-1.6,3.5-3.5 S9.9,1,8,1z\"/> <path d=\"M13,15h-1v-2.5c0-1.4-1.1-2.5-2.5-2.5h-3C5.1,10,4,11.1,4,12.5V15H3v-2.5C3,10.6,4.6,9,6.5,9h3c1.9,0,3.5,1.6,3.5,3.5V15z\" />"},"view--off-16":{"vb":"0 0 16 16","body":"<path d=\"M2.6,11.3l0.7-0.7C2.6,9.8,1.9,9,1.5,8c1-2.5,3.8-4.5,6.5-4.5c0.7,0,1.4,0.1,2,0.4l0.8-0.8C9.9,2.7,9,2.5,8,2.5 C4.7,2.6,1.7,4.7,0.5,7.8c0,0.1,0,0.2,0,0.3C1,9.3,1.7,10.4,2.6,11.3z\"/> <path d=\"M6,7.9c0.1-1,0.9-1.8,1.8-1.8l0.9-0.9C7.2,4.7,5.5,5.6,5.1,7.2C5,7.7,5,8.3,5.1,8.8L6,7.9z\"/> <path d=\"M15.5,7.8c-0.6-1.5-1.6-2.8-2.9-3.7L15,1.7L14.3,1L1,14.3L1.7,15l2.6-2.6c1.1,0.7,2.4,1,3.7,1.1c3.3-0.1,6.3-2.2,7.5-5.3 C15.5,8.1,15.5,7.9,15.5,7.8z M10,8c0,1.1-0.9,2-2,2c-0.3,0-0.7-0.1-1-0.3L9.7,7C9.9,7.3,10,7.6,10,8z M8,12.5c-1,0-2.1-0.3-3-0.8 l1.3-1.3c1.4,0.9,3.2,0.6,4.2-0.8c0.7-1,0.7-2.4,0-3.4l1.4-1.4c1.1,0.8,2,1.9,2.6,3.2C13.4,10.5,10.6,12.5,8,12.5z\"/>"},"view-16":{"vb":"0 0 16 16","body":"<path d=\"M15.5,7.8C14.3,4.7,11.3,2.6,8,2.5C4.7,2.6,1.7,4.7,0.5,7.8c0,0.1,0,0.2,0,0.3c1.2,3.1,4.1,5.2,7.5,5.3 c3.3-0.1,6.3-2.2,7.5-5.3C15.5,8.1,15.5,7.9,15.5,7.8z M8,12.5c-2.7,0-5.4-2-6.5-4.5c1-2.5,3.8-4.5,6.5-4.5s5.4,2,6.5,4.5 C13.4,10.5,10.6,12.5,8,12.5z\"/> <path d=\"M8,5C6.3,5,5,6.3,5,8s1.3,3,3,3s3-1.3,3-3S9.7,5,8,5z M8,10c-1.1,0-2-0.9-2-2s0.9-2,2-2s2,0.9,2,2S9.1,10,8,10z\"/>"},"warning--alt-16":{"vb":"0 0 16 16","body":"<path d=\"M14.5,14h-13c-0.2,0-0.3-0.1-0.4-0.2c-0.1-0.2-0.1-0.3,0-0.5l6.5-12C7.7,1,8,0.9,8.2,1.1c0.1,0,0.2,0.1,0.2,0.2l6.5,12 c0.1,0.2,0.1,0.3,0,0.5C14.9,13.9,14.7,14,14.5,14z M2.3,13h11.3L8,2.5L2.3,13z\"/> <rect x=\"7.5\" y=\"6\" width=\"1\" height=\"3.5\"/> <path d=\"M8,10.8c-0.4,0-0.8,0.3-0.8,0.8s0.3,0.8,0.8,0.8c0.4,0,0.8-0.3,0.8-0.8S8.4,10.8,8,10.8z\"/>"},"warning--filled-16":{"vb":"0 0 16 16","body":"<path id=\"Compound_Path\" d=\"M8,1C4.2,1,1,4.2,1,8s3.2,7,7,7s7-3.1,7-7S11.9,1,8,1z M7.5,4h1v5h-1C7.5,9,7.5,4,7.5,4z M8,12.2 c-0.4,0-0.8-0.4-0.8-0.8s0.3-0.8,0.8-0.8c0.4,0,0.8,0.4,0.8,0.8S8.4,12.2,8,12.2z\"/>"},"warning-16":{"vb":"0 0 16 16","body":"<path d=\"M8,1C4.1,1,1,4.1,1,8s3.1,7,7,7s7-3.1,7-7S11.9,1,8,1z M8,14c-3.3,0-6-2.7-6-6s2.7-6,6-6s6,2.7,6,6S11.3,14,8,14z\"/> <rect x=\"7.5\" y=\"4\" width=\"1\" height=\"5\"/> <path d=\"M8,10.2c-0.4,0-0.8,0.3-0.8,0.8s0.3,0.8,0.8,0.8c0.4,0,0.8-0.3,0.8-0.8S8.4,10.2,8,10.2z\"/>"},"arrow--right-20":{"vb":"0 0 20 20","body":"<polygon points=\"11.8,2.8 10.8,3.8 16.2,9.3 1,9.3 1,10.7 16.2,10.7 10.8,16.2 11.8,17.2 19,10 \"/>"},"checkmark--filled-20":{"vb":"0 0 20 20","body":"<path d=\"M10,1c-4.9,0-9,4.1-9,9s4.1,9,9,9s9-4,9-9S15,1,10,1z M8.7,13.5l-3.2-3.2l1-1l2.2,2.2l4.8-4.8l1,1L8.7,13.5z\"/>"},"error--filled-20":{"vb":"0 0 20 20","body":"<path d=\"M10,1c-5,0-9,4-9,9s4,9,9,9s9-4,9-9S15,1,10,1z M13.5,14.5l-8-8l1-1l8,8L13.5,14.5z\"/>"},"warning--filled-20":{"vb":"0 0 20 20","body":"<path id=\"Compound_Path\" d=\"M10,1c-5,0-9,4-9,9s4,9,9,9s9-4,9-9S15,1,10,1z M9.2,5h1.5v7H9.2V5z M10,16c-0.6,0-1-0.4-1-1s0.4-1,1-1 s1,0.4,1,1S10.6,16,10,16z\"/>"},"activity-32":{"vb":"0 0 32 32","body":"<path d=\"M12,29a1,1,0,0,1-.92-.62L6.33,17H2V15H7a1,1,0,0,1,.92.62L12,25.28,20.06,3.65A1,1,0,0,1,21,3a1,1,0,0,1,.93.68L25.72,15H30v2H25a1,1,0,0,1-.95-.68L21,7,12.94,28.35A1,1,0,0,1,12,29Z\"/>"},"add-32":{"vb":"0 0 32 32","body":"<polygon points=\"17,15 17,8 15,8 15,15 8,15 8,17 15,17 15,24 17,24 17,17 24,17 24,15 \"/>"},"ai-generate-32":{"vb":"0 0 32 32","body":"<path class=\"cls-2\" d=\"m19,22v-2h1v-7h-1v-2h4v2h-1v7h1v2h-4Z\"/> <path class=\"cls-2\" d=\"m15.5005,22h2l-3.5005-11h-3l-3.4966,11h1.9988l.6018-2h4.7781l.6184,2Zm-4.7947-4l1.6284-5.4111.2559-.0024,1.6736,5.4136h-3.5579Z\"/> <polygon class=\"cls-2\" points=\"32 4 28 4 28 0 26 0 26 4 22 4 22 6 26 6 26 10 28 10 28 6 32 6 32 4\"/> <polygon class=\"cls-2\" points=\"32 32 0 32 0 0 14 0 14 2 2 2 2 30 30 30 30 18 32 18 32 32\"/>"},"analytics-32":{"vb":"0 0 32 32","body":"<path d=\"M4,2H2V28a2,2,0,0,0,2,2H30V28H4Z\"/><path d=\"M30,9H23v2h3.59L19,18.59l-4.29-4.3a1,1,0,0,0-1.42,0L6,21.59,7.41,23,14,16.41l4.29,4.3a1,1,0,0,0,1.42,0L28,12.41V16h2Z\"/>"},"arrows--vertical-32":{"vb":"0 0 32 32","body":"<polygon points=\"27.6,20.6 24,24.2 24,4 22,4 22,24.2 18.4,20.6 17,22 23,28 29,22 \"/> <polygon points=\"9,4 3,10 4.4,11.4 8,7.8 8,28 10,28 10,7.8 13.6,11.4 15,10 \"/>"},"bee-32":{"vb":"0 0 32 32","body":"<path d=\"M16,10a6,6,0,0,0-6,6v8a6,6,0,0,0,12,0V16A6,6,0,0,0,16,10Zm-4.25,7.87h8.5v4.25h-8.5ZM16,28.25A4.27,4.27,0,0,1,11.75,24v-.13h8.5V24A4.27,4.27,0,0,1,16,28.25Zm4.25-12.13h-8.5V16a4.25,4.25,0,0,1,8.5,0Z\"/><path d=\"M30.66,19.21,24,13v9.1a4,4,0,0,0,8,0A3.83,3.83,0,0,0,30.66,19.21ZM28,24.35a2.25,2.25,0,0,1-2.25-2.25V17l3.72,3.47h0A2.05,2.05,0,0,1,30.2,22,2.25,2.25,0,0,1,28,24.35Z\"/><path d=\"M0,22.1a4,4,0,0,0,8,0V13L1.34,19.21A3.88,3.88,0,0,0,0,22.1Zm2.48-1.56h0L6.25,17v5.1a2.25,2.25,0,0,1-4.5,0A2.05,2.05,0,0,1,2.48,20.54Z\"/><path d=\"M15,5.5A3.5,3.5,0,1,0,11.5,9,3.5,3.5,0,0,0,15,5.5Zm-5.25,0A1.75,1.75,0,1,1,11.5,7.25,1.77,1.77,0,0,1,9.75,5.5Z\"/><path d=\"M20.5,2A3.5,3.5,0,1,0,24,5.5,3.5,3.5,0,0,0,20.5,2Zm0,5.25A1.75,1.75,0,1,1,22.25,5.5,1.77,1.77,0,0,1,20.5,7.25Z\"/>"},"calendar-32":{"vb":"0 0 32 32","body":"<path d=\"M26,4h-4V2h-2v2h-8V2h-2v2H6C4.9,4,4,4.9,4,6v20c0,1.1,0.9,2,2,2h20c1.1,0,2-0.9,2-2V6C28,4.9,27.1,4,26,4z M26,26H6V12h20 V26z M26,10H6V6h4v2h2V6h8v2h2V6h4V10z\"/>"},"caret--down-32":{"vb":"0 0 32 32","body":"<polygon points=\"24,12 16,22 8,12 \"/>"},"caret--left-32":{"vb":"0 0 32 32","body":"<polygon points=\"20,24 10,16 20,8 \"/>"},"caret--right-32":{"vb":"0 0 32 32","body":"<polygon points=\"12,8 22,16 12,24 \"/>"},"catalog-32":{"vb":"0 0 32 32","body":"<path d=\"M26,2H8A2,2,0,0,0,6,4V8H4v2H6v5H4v2H6v5H4v2H6v4a2,2,0,0,0,2,2H26a2,2,0,0,0,2-2V4A2,2,0,0,0,26,2Zm0,26H8V24h2V22H8V17h2V15H8V10h2V8H8V4H26Z\" transform=\"translate(0 0)\"/><rect x=\"14\" y=\"8\" width=\"8\" height=\"2\"/><rect x=\"14\" y=\"15\" width=\"8\" height=\"2\"/><rect x=\"14\" y=\"22\" width=\"8\" height=\"2\"/>"},"checkmark--filled-32":{"vb":"0 0 32 32","body":"<path d=\"M16,2A14,14,0,1,0,30,16,14,14,0,0,0,16,2ZM14,21.5908l-5-5L10.5906,15,14,18.4092,21.41,11l1.5957,1.5859Z\"/>"},"checkmark--outline-32":{"vb":"0 0 32 32","body":"<polygon points=\"14 21.414 9 16.413 10.413 15 14 18.586 21.585 11 23 12.415 14 21.414\"/> <path d=\"M16,2A14,14,0,1,0,30,16,14,14,0,0,0,16,2Zm0,26A12,12,0,1,1,28,16,12,12,0,0,1,16,28Z\"/>"},"checkmark-32":{"vb":"0 0 32 32","body":"<polygon points=\"13 24 4 15 5.414 13.586 13 21.171 26.586 7.586 28 9 13 24\"/>"},"circle-dash-32":{"vb":"0 0 32 32","body":"<path class=\"cls-1\" d=\"M7.7,4.7a14.7,14.7,0,0,0-3,3.1L6.3,9A13.26,13.26,0,0,1,8.9,6.3Z\"/><path class=\"cls-1\" d=\"M4.6,12.3l-1.9-.6A12.51,12.51,0,0,0,2,16H4A11.48,11.48,0,0,1,4.6,12.3Z\"/><path class=\"cls-1\" d=\"M2.7,20.4a14.4,14.4,0,0,0,2,3.9l1.6-1.2a12.89,12.89,0,0,1-1.7-3.3Z\"/><path class=\"cls-1\" d=\"M7.8,27.3a14.4,14.4,0,0,0,3.9,2l.6-1.9A12.89,12.89,0,0,1,9,25.7Z\"/><path class=\"cls-1\" d=\"M11.7,2.7l.6,1.9A11.48,11.48,0,0,1,16,4V2A12.51,12.51,0,0,0,11.7,2.7Z\"/><path class=\"cls-1\" d=\"M24.2,27.3a15.18,15.18,0,0,0,3.1-3.1L25.7,23A11.53,11.53,0,0,1,23,25.7Z\"/><path class=\"cls-1\" d=\"M27.4,19.7l1.9.6A15.47,15.47,0,0,0,30,16H28A11.48,11.48,0,0,1,27.4,19.7Z\"/><path class=\"cls-1\" d=\"M29.2,11.6a14.4,14.4,0,0,0-2-3.9L25.6,8.9a12.89,12.89,0,0,1,1.7,3.3Z\"/><path class=\"cls-1\" d=\"M24.1,4.6a14.4,14.4,0,0,0-3.9-2l-.6,1.9a12.89,12.89,0,0,1,3.3,1.7Z\"/><path class=\"cls-1\" d=\"M20.3,29.3l-.6-1.9A11.48,11.48,0,0,1,16,28v2A21.42,21.42,0,0,0,20.3,29.3Z\"/>"},"close-32":{"vb":"0 0 32 32","body":"<polygon points=\"17.4141 16 24 9.4141 22.5859 8 16 14.5859 9.4143 8 8 9.4141 14.5859 16 8 22.5859 9.4143 24 16 17.4141 22.5859 24 24 22.5859 17.4141 16\"/>"},"cloud-foundry--1-32":{"vb":"0 0 32 32","body":"<path d=\"M16,7h0a7.66,7.66,0,0,1,1.51.15,8,8,0,0,1,6.35,6.34l.26,1.35,1.35.24a5.5,5.5,0,0,1-1,10.92H7.5a5.5,5.5,0,0,1-1-10.92l1.34-.24.26-1.35A8,8,0,0,1,16,7m0-2a10,10,0,0,0-9.83,8.12A7.5,7.5,0,0,0,7.49,28h17a7.5,7.5,0,0,0,1.32-14.88,10,10,0,0,0-7.94-7.94A10.27,10.27,0,0,0,16,5Z\"/><path d=\"M14,24H11a2,2,0,0,1-2-2V19a2,2,0,0,1,2-2h3v2H11v3h3Z\"/><path d=\"M21,15V13H19a2,2,0,0,0-2,2v2H16v2h1v5h2V19h2V17H19V15Z\"/>"},"copy-32":{"vb":"0 0 32 32","body":"<path d=\"M28,10V28H10V10H28m0-2H10a2,2,0,0,0-2,2V28a2,2,0,0,0,2,2H28a2,2,0,0,0,2-2V10a2,2,0,0,0-2-2Z\" transform=\"translate(0)\"/><path d=\"M4,18H2V4A2,2,0,0,1,4,2H18V4H4Z\" transform=\"translate(0)\"/>"},"dashboard-32":{"vb":"0 0 32 32","body":"<rect x=\"24\" y=\"21\" width=\"2\" height=\"5\"/><rect x=\"20\" y=\"16\" width=\"2\" height=\"10\"/><path d=\"M11,26a5.0059,5.0059,0,0,1-5-5H8a3,3,0,1,0,3-3V16a5,5,0,0,1,0,10Z\"/><path d=\"M28,2H4A2.002,2.002,0,0,0,2,4V28a2.0023,2.0023,0,0,0,2,2H28a2.0027,2.0027,0,0,0,2-2V4A2.0023,2.0023,0,0,0,28,2Zm0,9H14V4H28ZM12,4v7H4V4ZM4,28V13H28.0007l.0013,15Z\"/>"},"document-32":{"vb":"0 0 32 32","body":"<path d=\"M25.7,9.3l-7-7C18.5,2.1,18.3,2,18,2H8C6.9,2,6,2.9,6,4v24c0,1.1,0.9,2,2,2h16c1.1,0,2-0.9,2-2V10C26,9.7,25.9,9.5,25.7,9.3 z M18,4.4l5.6,5.6H18V4.4z M24,28H8V4h8v6c0,1.1,0.9,2,2,2h6V28z\"/> <rect x=\"10\" y=\"22\" width=\"12\" height=\"2\"/> <rect x=\"10\" y=\"16\" width=\"12\" height=\"2\"/>"},"edit-32":{"vb":"0 0 32 32","body":"<rect x=\"2\" y=\"26\" width=\"28\" height=\"2\"/> <path d=\"M25.4,9c0.8-0.8,0.8-2,0-2.8c0,0,0,0,0,0l-3.6-3.6c-0.8-0.8-2-0.8-2.8,0c0,0,0,0,0,0l-15,15V24h6.4L25.4,9z M20.4,4L24,7.6 l-3,3L17.4,7L20.4,4z M6,22v-3.6l10-10l3.6,3.6l-10,10H6z\"/>"},"error--filled-32":{"vb":"0 0 32 32","body":"<path d=\"M16,2A13.914,13.914,0,0,0,2,16,13.914,13.914,0,0,0,16,30,13.914,13.914,0,0,0,30,16,13.914,13.914,0,0,0,16,2Zm5.4449,21L9,10.5557,10.5557,9,23,21.4448Z\"/>"},"filter-32":{"vb":"0 0 32 32","body":"<path d=\"M18,28H14a2,2,0,0,1-2-2V18.41L4.59,11A2,2,0,0,1,4,9.59V6A2,2,0,0,1,6,4H26a2,2,0,0,1,2,2V9.59A2,2,0,0,1,27.41,11L20,18.41V26A2,2,0,0,1,18,28ZM6,6V9.59l8,8V26h4V17.59l8-8V6Z\"/>"},"folder--open-32":{"vb":"0 0 32 32","body":"<path d=\"M28,8H20.8284L17.4143,4.5859A2,2,0,0,0,16,4H4A2,2,0,0,0,2,6V26a2,2,0,0,0,2,2H28a2,2,0,0,0,2-2V10A2,2,0,0,0,28,8ZM8,26V14h8v6.17l-2.59-2.58L12,19l5,5,5-5-1.41-1.41L18,20.17V14a2.0025,2.0025,0,0,0-2-2H8a2.0025,2.0025,0,0,0-2,2V26H4V6H16l4,4h8v2H22v2h6V26Z\"/>"},"folder-32":{"vb":"0 0 32 32","body":"<path d=\"M11.17,6l3.42,3.41.58.59H28V26H4V6h7.17m0-2H4A2,2,0,0,0,2,6V26a2,2,0,0,0,2,2H28a2,2,0,0,0,2-2V10a2,2,0,0,0-2-2H16L12.59,4.59A2,2,0,0,0,11.17,4Z\" transform=\"translate(0)\"/>"},"folders-32":{"vb":"0 0 32 32","body":"<path d=\"M26,28H6a2.0021,2.0021,0,0,1-2-2V11A2.0021,2.0021,0,0,1,6,9h5.6665a2.0119,2.0119,0,0,1,1.2007.4L16.3335,12H26a2.0021,2.0021,0,0,1,2,2V26A2.0021,2.0021,0,0,1,26,28ZM11.6665,11H5.9985L6,26H26V14H15.6665Z\"/><path d=\"M28,9H17.6665l-4-3H6V4h7.6665a2.0119,2.0119,0,0,1,1.2007.4L18.3335,7H28Z\"/>"},"home-32":{"vb":"0 0 32 32","body":"<path d=\"M16.6123,2.2138a1.01,1.01,0,0,0-1.2427,0L1,13.4194l1.2427,1.5717L4,13.6209V26a2.0041,2.0041,0,0,0,2,2H26a2.0037,2.0037,0,0,0,2-2V13.63L29.7573,15,31,13.4282ZM18,26H14V18h4Zm2,0V18a2.0023,2.0023,0,0,0-2-2H14a2.002,2.002,0,0,0-2,2v8H6V12.0615l10-7.79,10,7.8005V26Z\" transform=\"translate(0 0)\"/>"},"in-progress-32":{"vb":"0 0 32 32","body":"<path d=\"M16,2A14,14,0,1,0,30,16,14.0158,14.0158,0,0,0,16,2Zm0,26A12,12,0,0,1,16,4V16l8.4812,8.4814A11.9625,11.9625,0,0,1,16,28Z\"/>"},"incomplete-32":{"vb":"0 0 32 32","body":"<path d=\"M23.7642,6.8593l1.2851-1.5315A13.976,13.976,0,0,0,20.8672,2.887l-.6836,1.8776A11.9729,11.9729,0,0,1,23.7642,6.8593Z\" transform=\"translate(0 0)\"/> <path d=\"M27.81,14l1.9677-.4128A13.8888,13.8888,0,0,0,28.14,9.0457L26.4087,10A12.52,12.52,0,0,1,27.81,14Z\" transform=\"translate(0 0)\"/> <path d=\"M20.1836,27.2354l.6836,1.8776a13.976,13.976,0,0,0,4.1821-2.4408l-1.2851-1.5315A11.9729,11.9729,0,0,1,20.1836,27.2354Z\" transform=\"translate(0 0)\"/> <path d=\"M26.4087,22,28.14,23a14.14,14.14,0,0,0,1.6382-4.5872L27.81,18.0659A12.1519,12.1519,0,0,1,26.4087,22Z\" transform=\"translate(0 0)\"/> <path d=\"M16,30V2a14,14,0,0,0,0,28Z\" transform=\"translate(0 0)\"/>"},"information--filled-32":{"vb":"0 0 32 32","body":"<path d=\"M16,2A14,14,0,1,0,30,16,14,14,0,0,0,16,2Zm0,6a1.5,1.5,0,1,1-1.5,1.5A1.5,1.5,0,0,1,16,8Zm4,16.125H12v-2.25h2.875v-5.75H13v-2.25h4.125v8H20Z\" transform=\"translate(0 0)\"/>"},"information--square--filled-32":{"vb":"0 0 32 32","body":"<path d=\"M26,4H6A2,2,0,0,0,4,6V26a2,2,0,0,0,2,2H26a2,2,0,0,0,2-2V6A2,2,0,0,0,26,4ZM16,8a1.5,1.5,0,1,1-1.5,1.5A1.5,1.5,0,0,1,16,8Zm4,16.125H12v-2.25h2.875v-5.75H13v-2.25h4.125v8H20Z\"/>"},"information-32":{"vb":"0 0 32 32","body":"<polygon points=\"17 22 17 14 13 14 13 16 15 16 15 22 12 22 12 24 20 24 20 22 17 22\"/> <path d=\"M16,8a1.5,1.5,0,1,0,1.5,1.5A1.5,1.5,0,0,0,16,8Z\"/> <path d=\"M16,30A14,14,0,1,1,30,16,14,14,0,0,1,16,30ZM16,4A12,12,0,1,0,28,16,12,12,0,0,0,16,4Z\"/>"},"overflow-menu--horizontal-32":{"vb":"0 0 32 32","body":"<circle cx=\"8\" cy=\"16\" r=\"2\"/> <circle cx=\"16\" cy=\"16\" r=\"2\"/> <circle cx=\"24\" cy=\"16\" r=\"2\"/>"},"overflow-menu--vertical-32":{"vb":"0 0 32 32","body":"<circle cx=\"16\" cy=\"8\" r=\"2\"/> <circle cx=\"16\" cy=\"16\" r=\"2\"/> <circle cx=\"16\" cy=\"24\" r=\"2\"/>"},"pending--filled-32":{"vb":"0 0 32 32","body":"<path d=\"M16,2A14,14,0,1,0,30,16,14,14,0,0,0,16,2ZM8,18a2,2,0,1,1,2-2A2,2,0,0,1,8,18Zm8,0a2,2,0,1,1,2-2A2,2,0,0,1,16,18Zm8,0a2,2,0,1,1,2-2A2,2,0,0,1,24,18Z\" transform=\"translate(0 0)\"/>"},"radio-button--checked-32":{"vb":"0 0 32 32","body":"<path d=\"M16,2A14,14,0,1,0,30,16,14,14,0,0,0,16,2Zm0,26A12,12,0,1,1,28,16,12,12,0,0,1,16,28Z\"/><path d=\"M16,10a6,6,0,1,0,6,6A6,6,0,0,0,16,10Z\"/>"},"radio-button-32":{"vb":"0 0 32 32","body":"<path d=\"M16,2A14,14,0,1,0,30,16,14,14,0,0,0,16,2Zm0,26A12,12,0,1,1,28,16,12,12,0,0,1,16,28Z\"/>"},"report-32":{"vb":"0 0 32 32","body":"<rect x=\"10\" y=\"18\" width=\"8\" height=\"2\"/><rect x=\"10\" y=\"13\" width=\"12\" height=\"2\"/><rect x=\"10\" y=\"23\" width=\"5\" height=\"2\"/><path d=\"M25,5H22V4a2,2,0,0,0-2-2H12a2,2,0,0,0-2,2V5H7A2,2,0,0,0,5,7V28a2,2,0,0,0,2,2H25a2,2,0,0,0,2-2V7A2,2,0,0,0,25,5ZM12,4h8V8H12ZM25,28H7V7h3v3H22V7h3Z\"/>"},"settings--adjust-32":{"vb":"0 0 32 32","body":"<path d=\"M30,8h-4.1c-0.5-2.3-2.5-4-4.9-4s-4.4,1.7-4.9,4H2v2h14.1c0.5,2.3,2.5,4,4.9,4s4.4-1.7,4.9-4H30V8z M21,12c-1.7,0-3-1.3-3-3 s1.3-3,3-3s3,1.3,3,3S22.7,12,21,12z\"/> <path d=\"M2,24h4.1c0.5,2.3,2.5,4,4.9,4s4.4-1.7,4.9-4H30v-2H15.9c-0.5-2.3-2.5-4-4.9-4s-4.4,1.7-4.9,4H2V24z M11,20c1.7,0,3,1.3,3,3 s-1.3,3-3,3s-3-1.3-3-3S9.3,20,11,20z\"/>"},"share-32":{"vb":"0 0 32 32","body":"<path d=\"M23,20a5,5,0,0,0-3.89,1.89L11.8,17.32a4.46,4.46,0,0,0,0-2.64l7.31-4.57A5,5,0,1,0,18,7a4.79,4.79,0,0,0,.2,1.32l-7.31,4.57a5,5,0,1,0,0,6.22l7.31,4.57A4.79,4.79,0,0,0,18,25a5,5,0,1,0,5-5ZM23,4a3,3,0,1,1-3,3A3,3,0,0,1,23,4ZM7,19a3,3,0,1,1,3-3A3,3,0,0,1,7,19Zm16,9a3,3,0,1,1,3-3A3,3,0,0,1,23,28Z\"/>"},"switcher-32":{"vb":"0 0 32 32","body":"<rect x=\"14\" y=\"4\" width=\"4\" height=\"4\"/> <rect x=\"4\" y=\"4\" width=\"4\" height=\"4\"/> <rect x=\"24\" y=\"4\" width=\"4\" height=\"4\"/> <rect x=\"14\" y=\"14\" width=\"4\" height=\"4\"/> <rect x=\"4\" y=\"14\" width=\"4\" height=\"4\"/> <rect x=\"24\" y=\"14\" width=\"4\" height=\"4\"/> <rect x=\"14\" y=\"24\" width=\"4\" height=\"4\"/> <rect x=\"4\" y=\"24\" width=\"4\" height=\"4\"/> <rect x=\"24\" y=\"24\" width=\"4\" height=\"4\"/>"},"undo-32":{"vb":"0 0 32 32","body":"<path d=\"M20,10H7.8149l3.5874-3.5859L10,5,4,11,10,17l1.4023-1.4146L7.8179,12H20a6,6,0,0,1,0,12H12v2h8a8,8,0,0,0,0-16Z\"/>"},"virtual-machine-32":{"vb":"0 0 32 32","body":"<path d=\"M20,13H12a2.0025,2.0025,0,0,0-2,2V28a2.0025,2.0025,0,0,0,2,2h8a2.0025,2.0025,0,0,0,2-2V15A2.0025,2.0025,0,0,0,20,13Zm0,2,0,3H12V15Zm0,5,0,3H12V20Zm-8,8V25h8v3Z\"/><path d=\"M25.91,10.13a.121.121,0,0,1-.0967-.0952A10.0061,10.0061,0,0,0,17.89,2.1816,10.0025,10.0025,0,0,0,6.1858,10.0347a.1212.1212,0,0,1-.0964.0957A7.5019,7.5019,0,0,0,7.4912,25H8V23H7.4954a5.5108,5.5108,0,0,1-5.4387-6.3,5.6992,5.6992,0,0,1,4.7138-4.6606l1.0166-.1836a.1306.1306,0,0,0,.1045-.1035l.18-.9351a8.28,8.28,0,0,1,6.8469-6.7427,7.957,7.957,0,0,1,2.8471.1245,8.22,8.22,0,0,1,6.1475,6.545l.1941,1.0083a.13.13,0,0,0,.1045.1035l1.0576.1914a5.7819,5.7819,0,0,1,3.1011,1.539A5.5052,5.5052,0,0,1,24.5076,23H24v2h.5076A7.5019,7.5019,0,0,0,25.91,10.13Z\"/>"},"warning--alt--filled-32":{"vb":"0 0 32 32","body":"<path d=\"M16.002,6.1714h-.004L4.6487,27.9966,4.6506,28H27.3494l.0019-.0034ZM14.875,12h2.25v9h-2.25ZM16,26a1.5,1.5,0,1,1,1.5-1.5A1.5,1.5,0,0,1,16,26Z\"/> <path d=\"M29,30H3a1,1,0,0,1-.8872-1.4614l13-25a1,1,0,0,1,1.7744,0l13,25A1,1,0,0,1,29,30ZM4.6507,28H27.3493l.002-.0033L16.002,6.1714h-.004L4.6487,27.9967Z\"/>"}};

  // Icon: Carbon's own SVG glyphs (packages/icons), inlined. name = file stem, e.g. "add", "close", "warning--filled".
  // 'add' and 'close' ship at 32 only. Carbon's 16px close is the 32 artwork at half scale (an 8px cross), so close keeps
  // the full box; Carbon's 16px add is a 10px cross, so add crops to the centre 26x26 (about 10px at size 16).
  var CROP = { 'add-32': '3 3 26 26' };
  // 32px-only glyphs whose artwork sits inside the centre 24x24 are cropped to it when drawn smaller, so they keep a
  // sensible optical size. Glyphs whose artwork reaches the 2px edge of the 32 box keep the full box (cropping clipped them).
  var FIT24 = { 'caret--down': 1, 'caret--left': 1, 'caret--right': 1, 'checkmark': 1, 'filter': 1, 'folders': 1, 'information--square--filled': 1, 'overflow-menu--horizontal': 1, 'overflow-menu--vertical': 1, 'switcher': 1, 'undo': 1 };
  function Icon(props) {
    var name = props.name, size = props.size || 16;
    var cand = [size, 16, 20, 24, 32], key = null;
    for (var i = 0; i < cand.length; i++) { if (ICONS[name + '-' + cand[i]]) { key = name + '-' + cand[i]; break; } }
    var def = key && ICONS[key];
    if (!def) return null;
    var vb = CROP[key] || (/-32$/.test(key) && size < 32 && FIT24[name] ? '4 4 24 24' : def.vb);
    return h('svg', {
      className: 'cds--icon' + (props.className ? ' ' + props.className : ''),
      width: size, height: size, viewBox: vb, focusable: 'false',
      'aria-hidden': props.title ? undefined : 'true', role: props.title ? 'img' : undefined,
      'aria-label': props.title, style: props.style,
      dangerouslySetInnerHTML: memoHtml(def)
    });
  }
  // Stable {__html} objects so re-renders never reset an icon's markup mid-click.
  var HTMLC = {};
  function htmlOf(s) { return HTMLC[s] || (HTMLC[s] = { __html: s }); }
  function memoHtml(def) { return def.html || (def.html = { __html: def.body }); }

  function cx() { return Array.prototype.filter.call(arguments, Boolean).join(' '); }
  // Copy every prop not named in `skip` (aria-*, data-*, name, onBlur...) so it reaches the native element.
  function omit(props, skip) { var o = {}; for (var k in props) { if (skip.indexOf(k) < 0) o[k] = props[k]; } return o; }
  function el(tag, attrs, kids) { return h.apply(null, [tag, attrs].concat(kids)); }

  // WarningAltFilled (Carbon's warn-state glyph) is not in the icon subset; a 16px triangle with a cut-out mark.
  function WarnIcon(props) {
    return h('svg', {
      className: 'cds--icon ' + props.className, width: 16, height: 16, viewBox: '0 0 16 16', focusable: 'false', 'aria-hidden': 'true',
      dangerouslySetInnerHTML: htmlOf('<path fill-rule="evenodd" d="M8 1L15.5 14.5H0.5L8 1ZM7.5 6V10.5H8.5V6H7.5ZM8 11.4C7.55 11.4 7.2 11.75 7.2 12.2C7.2 12.65 7.55 13 8 13C8.45 13 8.8 12.65 8.8 12.2C8.8 11.75 8.45 11.4 8 11.4Z"/>')
    });
  }

  // Button -----------------------------------------------------------------
  var BTN_KEYS = ['kind', 'size', 'renderIcon', 'hasIconOnly', 'iconDescription', 'isExpressive', 'isSelected', 'href', 'type', 'disabled', 'onClick', 'className', 'children', 'tooltipPosition', 'tooltipAlignment'];
  function Button(props) {
    var kind = props.kind || 'primary', size = props.size || 'lg';
    var icon = props.renderIcon, onlyIcon = props.hasIconOnly;
    var selected = onlyIcon && kind === 'ghost' && props.isSelected;
    var cls = cx('cds--btn', 'cds--btn--' + kind, kind === 'danger--primary' && 'cds--btn--danger', 'cds--btn--' + size,
      props.isExpressive && 'cds--btn--expressive', onlyIcon && 'cds--btn--icon-only', selected && 'cds--btn--selected', props.className);
    var iconEl = icon ? h(Icon, { key: 'i', name: icon, size: props.isExpressive && !onlyIcon ? 20 : 16, className: 'cds--btn__icon' }) : null;
    var kids = onlyIcon
      ? [iconEl, h('span', { key: 'l', className: 'cds--visually-hidden' }, props.iconDescription || props.children)]
      : [props.children, iconEl];
    var common = Object.assign(omit(props, BTN_KEYS), { className: cls, title: onlyIcon ? props.iconDescription : props.title });
    if (onlyIcon && kind === 'ghost' && props.isSelected !== undefined) common['aria-pressed'] = !!props.isSelected;
    if (props.href && !props.disabled) {
      return el('a', Object.assign(common, { href: props.href, onClick: props.onClick }), kids);
    }
    return el('button', Object.assign(common, { type: props.type || 'button', disabled: props.disabled, onClick: props.onClick }), kids);
  }

  // Tag --------------------------------------------------------------------
  var TAG_KEYS = ['type', 'size', 'filter', 'onClose', 'onClick', 'disabled', 'renderIcon', 'title', 'className', 'children'];
  function Tag(props) {
    var type = props.type || 'gray', size = props.size || 'md';
    var cls = cx('cds--tag', 'cds--tag--' + size, 'cds--tag--' + type, props.onClick && 'cds--tag--operational', props.filter && 'cds--tag--filter', props.disabled && 'cds--tag--disabled', props.className);
    var text = typeof props.children === 'string' ? props.children : undefined;
    var icon = props.renderIcon && size !== 'sm' ? h('div', { className: 'cds--tag__custom-icon' }, h(Icon, { name: props.renderIcon, size: 16 })) : null;
    var label = h('span', { className: 'cds--tag__label', title: text }, props.children);
    var close = props.filter ? h('button', { type: 'button', className: 'cds--tag__close-icon', onClick: props.onClose, disabled: props.disabled, 'aria-label': props.title || 'Clear filter', title: props.title || 'Clear filter' }, h(Icon, { name: 'close', size: 16 })) : null;
    var rest = omit(props, TAG_KEYS);
    if (props.onClick) return h('button', Object.assign(rest, { type: 'button', className: cls, onClick: props.onClick, disabled: props.disabled }), icon, label);
    return h('span', Object.assign(rest, { className: cls }), icon, label, close);
  }

  // TextInput --------------------------------------------------------------
  var TI_KEYS = ['id', 'labelText', 'hideLabel', 'helperText', 'invalid', 'invalidText', 'warn', 'warnText', 'placeholder', 'size', 'type', 'value', 'defaultValue', 'disabled', 'readOnly', 'onChange', 'className', 'inline', 'enableCounter', 'maxCount', 'light'];
  function TextInput(props) {
    var autoId = useId(); var id = props.id || ('ti-' + autoId);
    var size = props.size || 'md', disabled = !!props.disabled, readOnly = !!props.readOnly, inline = !!props.inline;
    // Carbon's normalizeProps: read-only and disabled fields never show invalid or warn; invalid wins over warn.
    var invalid = !!props.invalid && !readOnly && !disabled;
    var warn = !invalid && !!props.warn && !readOnly && !disabled;
    var st = useState(function () { return String(props.defaultValue != null ? props.defaultValue : '').length; });
    var count = props.value != null ? String(props.value).length : st[0];
    var counter = props.enableCounter && props.maxCount;
    function change(e) { if (props.value == null) st[1](e.target.value.length); if (props.onChange) props.onChange(e); }
    var msgId = id + '-msg', helpId = id + '-help';
    var helper = props.helperText && !invalid && !warn
      ? h('div', { id: helpId, className: cx('cds--form__helper-text', disabled && 'cds--form__helper-text--disabled', inline && 'cds--form__helper-text--inline') }, props.helperText) : null;
    var inputProps = Object.assign(omit(props, TI_KEYS), {
      id: id, type: props.type || 'text', placeholder: props.placeholder, disabled: disabled,
      value: props.value, defaultValue: props.defaultValue, onChange: change, readOnly: readOnly,
      maxLength: counter ? props.maxCount : props.maxLength,
      'aria-invalid': invalid || undefined, 'aria-readonly': readOnly || undefined,
      'aria-describedby': props['aria-describedby'] || ((invalid || warn) ? msgId : (helper ? helpId : undefined)),
      className: cx('cds--text-input', size !== 'md' && 'cds--text-input--' + size, invalid && 'cds--text-input--invalid', warn && 'cds--text-input--warning')
    });
    var label = props.labelText ? h('label', { htmlFor: id, className: cx('cds--label', props.hideLabel && 'cds--visually-hidden', disabled && 'cds--label--disabled', inline && 'cds--label--inline', inline && 'cds--label--inline--' + size) }, props.labelText) : null;
    var counterEl = counter && !inline ? h('div', { className: cx('cds--label', 'cds--text-input__label-counter', disabled && 'cds--label--disabled'), 'aria-hidden': 'true' }, count + '/' + props.maxCount) : null;
    var labelRow = counterEl ? h('div', { className: 'cds--text-input__label-wrapper' }, label, counterEl) : label;
    var msg = invalid ? h('div', { id: msgId, className: 'cds--form-requirement' }, props.invalidText)
      : warn ? h('div', { id: msgId, className: 'cds--form-requirement cds--form-requirement--warn' }, props.warnText) : null;
    var field = h('div', { className: cx('cds--text-input__field-wrapper', warn && 'cds--text-input__field-wrapper--warning'), 'data-invalid': invalid || undefined },
      h('input', inputProps),
      invalid ? h(Icon, { name: 'warning--filled', size: 16, className: 'cds--text-input__invalid-icon' }) : null,
      warn ? h(WarnIcon, { className: 'cds--text-input__invalid-icon cds--text-input__invalid-icon--warning' }) : null);
    var wrapCls = cx('cds--text-input-wrapper', inline && 'cds--text-input-wrapper--inline', readOnly && 'cds--text-input-wrapper--readonly');
    if (inline) {
      return h('div', { className: cx('cds--form-item', 'cds--text-input-wrapper--inline-form-item', props.className) },
        h('div', { className: wrapCls },
          h('div', { className: 'cds--text-input__label-helper-wrapper' }, label, helper),
          h('div', { className: 'cds--text-input__field-outer-wrapper cds--text-input__field-outer-wrapper--inline' }, field, msg)));
    }
    return h('div', { className: cx('cds--form-item', props.className) },
      h('div', { className: wrapCls }, labelRow, field, msg, helper));
  }

  // Checkbox ---------------------------------------------------------------
  var CB_KEYS = ['id', 'labelText', 'hideLabel', 'helperText', 'invalid', 'invalidText', 'warn', 'warnText', 'readOnly', 'checked', 'defaultChecked', 'indeterminate', 'disabled', 'onChange', 'onClick', 'className', 'title'];
  function Checkbox(props) {
    var autoId = useId(); var id = props.id || ('cb-' + autoId);
    var ref = useRef(null);
    var readOnly = !!props.readOnly, invalid = !!props.invalid && !readOnly && !props.disabled;
    var warn = !invalid && !!props.warn && !readOnly && !props.disabled;
    // Re-apply on every render: a click clears the native indeterminate flag even when the prop stays true.
    useEffect(function () { if (ref.current) ref.current.indeterminate = !!props.indeterminate; });
    function change(e) { if (readOnly) return; if (props.onChange) props.onChange(e, { checked: e.target.checked, id: id }); }
    function click(e) { if (readOnly) e.preventDefault(); if (props.onClick) props.onClick(e); }
    var helpId = id + '-help', msgId = id + '-msg';
    var showHelper = props.helperText && !invalid && !warn;
    var input = Object.assign(omit(props, CB_KEYS), {
      ref: ref, id: id, type: 'checkbox', className: 'cds--checkbox', checked: props.checked, defaultChecked: props.defaultChecked,
      disabled: props.disabled, onChange: change, onClick: click, 'aria-readonly': readOnly || undefined, 'aria-invalid': invalid || undefined,
      'aria-checked': props.indeterminate ? 'mixed' : undefined,
      'aria-describedby': props['aria-describedby'] || ((invalid || warn) ? msgId : (showHelper ? helpId : undefined))
    });
    return h('div', { className: cx('cds--checkbox-wrapper', readOnly && 'cds--checkbox-wrapper--readonly', invalid && 'cds--checkbox-wrapper--invalid', warn && 'cds--checkbox-wrapper--warning', props.className) },
      h('input', input),
      h('label', { htmlFor: id, className: 'cds--checkbox-label', title: props.title },
        h('span', { className: cx('cds--checkbox-label-text', props.hideLabel && 'cds--visually-hidden') }, props.labelText)),
      (invalid || warn) ? h('div', { className: 'cds--checkbox__validation-msg' },
        invalid ? h(Icon, { name: 'warning--filled', size: 16, className: 'cds--checkbox__invalid-icon' }) : h(WarnIcon, { className: 'cds--checkbox__invalid-icon cds--checkbox__invalid-icon--warning' }),
        h('div', { id: msgId, className: 'cds--form-requirement' }, invalid ? props.invalidText : props.warnText)) : null,
      showHelper ? h('div', { id: helpId, className: cx('cds--form__helper-text', props.disabled && 'cds--form__helper-text--disabled') }, props.helperText) : null);
  }

  // Toggle -----------------------------------------------------------------
  var TG_KEYS = ['id', 'labelText', 'labelA', 'labelB', 'hideLabel', 'toggled', 'defaultToggled', 'onToggle', 'onClick', 'size', 'disabled', 'readOnly', 'className'];
  function Toggle(props) {
    var autoId = useId(); var id = props.id || ('tg-' + autoId);
    var st = useState(!!props.defaultToggled); var inner = st[0], setInner = st[1];
    var controlled = props.toggled !== undefined; var on = controlled ? !!props.toggled : inner;
    var sm = props.size === 'sm', readOnly = !!props.readOnly, hide = !!props.hideLabel;
    // Carbon: hideLabel visually hides the label above and shows labelText beside the switch instead of the state text.
    var side = hide ? props.labelText : (on ? (props.labelB != null ? props.labelB : 'On') : (props.labelA != null ? props.labelA : 'Off'));
    function click(e) {
      if (props.onClick) props.onClick(e);
      if (props.disabled || readOnly) return;
      if (!controlled) setInner(!on);
      if (props.onToggle) props.onToggle(!on);
    }
    var lblId = id + '-lbl';
    return h('div', { className: cx('cds--toggle', sm && 'cds--toggle--sm', props.disabled && 'cds--toggle--disabled', readOnly && 'cds--toggle--readonly', props.className) },
      h('button', Object.assign(omit(props, TG_KEYS), { id: id, type: 'button', role: 'switch', 'aria-checked': on, 'aria-readonly': readOnly || undefined, 'aria-labelledby': props['aria-labelledby'] || (props.labelText ? lblId : undefined), disabled: props.disabled, className: 'cds--toggle__button', onClick: click })),
      h('label', { htmlFor: id, className: 'cds--toggle__label' },
        props.labelText ? h('span', { className: cx('cds--toggle__label-text', hide && 'cds--visually-hidden'), id: lblId }, props.labelText) : null,
        h('div', { className: cx('cds--toggle__appearance', sm && 'cds--toggle__appearance--sm') },
          h('div', { className: cx('cds--toggle__switch', on && 'cds--toggle__switch--checked') },
            sm && !readOnly ? h('svg', { className: 'cds--toggle__check', viewBox: '0 0 6 5', 'aria-hidden': 'true' }, h('path', { d: 'M2.2 2.7L5 0 6 1 2.2 5 0 2.7 1 1.5z' })) : null),
          side ? h('span', { className: 'cds--toggle__text', 'aria-hidden': 'true' }, side) : null)));
  }

  // InlineNotification -----------------------------------------------------
  // Carbon's glyphs per kind; information--filled, information--square--filled and warning--alt--filled ship at 32 only.
  var NOTE_ICON = { error: 'error--filled', success: 'checkmark--filled', warning: 'warning--filled', 'warning-alt': 'warning--alt--filled', info: 'information--filled', 'info-square': 'information--square--filled' };
  function InlineNotification(props) {
    var kind = NOTE_ICON[props.kind] ? props.kind : 'info';
    var st = useState(true); var open = st[0], setOpen = st[1];
    if (!open) return null;
    function close(e) {
      if (!props.onClose || props.onClose(e) !== false) setOpen(false);
      if (props.onCloseButtonClick) props.onCloseButtonClick(e);
    }
    function key(e) { if (e.key === 'Escape' && !props.hideCloseButton) close(e); }
    var rest = omit(props, ['kind', 'title', 'subtitle', 'children', 'lowContrast', 'hideCloseButton', 'onClose', 'onCloseButtonClick', 'actions', 'role', 'statusIconDescription', 'ariaLabel', 'className']);
    return h('div', Object.assign(rest, { role: props.role || 'status', onKeyDown: key, className: cx('cds--inline-notification', 'cds--inline-notification--' + kind, props.lowContrast && 'cds--inline-notification--low-contrast', props.hideCloseButton && 'cds--inline-notification--hide-close-button', props.className) }),
      h('div', { className: 'cds--inline-notification__details' },
        h(Icon, { name: NOTE_ICON[kind], size: 20, title: props.statusIconDescription || (kind.replace('-', ' ') + ' icon'), className: 'cds--inline-notification__icon' }),
        h('div', { className: 'cds--inline-notification__text-wrapper' },
          props.title ? h('div', { className: 'cds--inline-notification__title' }, props.title) : null,
          props.subtitle ? h('div', { className: 'cds--inline-notification__subtitle' }, props.subtitle) : null,
          props.children)),
      props.actions ? h('div', { className: 'cds--inline-notification__action-button' }, props.actions) : null,
      props.hideCloseButton ? null : h('button', { type: 'button', className: 'cds--inline-notification__close-button', 'aria-label': props.ariaLabel || 'Close notification', title: props.ariaLabel || 'Close notification', onClick: close }, h(Icon, { name: 'close', size: 16, className: 'cds--inline-notification__close-icon' })));
  }

  // Tile -------------------------------------------------------------------
  function Tile(props) {
    var clickable = !!props.href || !!props.onClick;
    var cls = cx('cds--tile', clickable && 'cds--tile--clickable', props.disabled && 'cds--tile--disabled', props.className);
    var rest = omit(props, ['href', 'onClick', 'disabled', 'className', 'children', 'light']);
    if (props.href && !props.disabled) return h('a', Object.assign(rest, { className: cls, href: props.href }), props.children, h(Icon, { name: 'arrow--right', size: 20, className: 'cds--tile__chevron' }));
    if (props.onClick && !props.disabled) return h('button', Object.assign(rest, { type: 'button', className: cx(cls, 'cds--tile--button'), onClick: props.onClick }), props.children);
    return h('div', Object.assign(rest, { className: cls, 'aria-disabled': props.disabled && clickable ? 'true' : undefined }), props.children);
  }

  // ClickableTile: the whole tile navigates (href) or acts (onClick). Trailing icon bottom-right in icon-interactive.
  function ClickableTile(props) {
    var dis = !!props.disabled;
    var iconName = props.renderIcon === undefined ? 'arrow--right' : props.renderIcon;
    var cls = cx('cds--tile', 'cds--tile--clickable', dis && 'cds--tile--disabled', props.className);
    var icon = iconName ? h(Icon, { name: iconName, size: 20, className: 'cds--tile--icon' }) : null;
    var rest = omit(props, ['href', 'onClick', 'disabled', 'renderIcon', 'className', 'children', 'light', 'target', 'rel']);
    if (dis) return h('div', Object.assign(rest, { className: cls, 'aria-disabled': 'true' }), props.children, icon);
    if (props.href) return h('a', Object.assign(rest, { className: cls, href: props.href, target: props.target, rel: props.rel || (props.target === '_blank' ? 'noopener noreferrer' : undefined), onClick: props.onClick }), props.children, icon);
    return h('button', Object.assign(rest, { type: 'button', className: cx(cls, 'cds--tile--button'), onClick: props.onClick }), props.children, icon);
  }

  // Link -------------------------------------------------------------------
  function Link(props) {
    var cls = cx('cds--link', props.size && props.size !== 'md' && 'cds--link--' + props.size, props.inline && 'cds--link--inline', props.visited && 'cds--link--visited', props.disabled && 'cds--link--disabled', props.className);
    // Carbon never renders an icon on an inline link.
    var icon = props.renderIcon && !props.inline ? h('span', { className: 'cds--link__icon' }, h(Icon, { name: props.renderIcon, size: 16 })) : null;
    var rest = omit(props, ['href', 'target', 'rel', 'size', 'inline', 'visited', 'disabled', 'renderIcon', 'onClick', 'className', 'children']);
    if (props.disabled) return h('a', Object.assign(rest, { className: cls, role: 'link', 'aria-disabled': 'true' }), props.children, icon);
    return h('a', Object.assign(rest, { className: cls, href: props.href || '#', target: props.target, rel: props.rel || (props.target === '_blank' ? 'noopener noreferrer' : undefined), onClick: props.onClick }), props.children, icon);
  }

  // Tabs -------------------------------------------------------------------
  function Tabs(props) {
    var items = props.items || [];
    var controlled = props.selectedIndex !== undefined && typeof props.onChange === 'function';
    var st = useState(function () { return props.defaultSelectedIndex != null ? props.defaultSelectedIndex : (props.selectedIndex || 0); });
    var idx = Math.max(0, Math.min(controlled ? props.selectedIndex : st[0], items.length - 1));
    var fs = useState(null); var focusIdx = fs[0] != null && fs[0] < items.length ? fs[0] : idx, setFocus = fs[1];
    var manual = props.activation === 'manual';
    var autoId = useId();
    var navRef = useRef(null);
    var iconOnly = !!props.iconOnly;
    var tall = props.contained && items.some(function (it) { return it.secondaryLabel; });
    function select(i) {
      if (!items[i] || items[i].disabled) return;
      if (!controlled) st[1](i);
      setFocus(null);
      if (i !== idx && props.onChange) props.onChange({ selectedIndex: i });
    }
    function focusTab(n) { var btns = navRef.current ? navRef.current.querySelectorAll('[role=tab]') : []; if (btns[n]) btns[n].focus(); }
    function step(from, dir) {
      var n = from; for (var i = 0; i < items.length; i++) { n = (n + dir + items.length) % items.length; if (!items[n].disabled) return n; }
      return from;
    }
    function edge(first) {
      for (var i = 0; i < items.length; i++) { var n = first ? i : items.length - 1 - i; if (!items[n].disabled) return n; }
      return focusIdx;
    }
    function onKey(e) {
      var n = null;
      if (e.key === 'ArrowRight') n = step(focusIdx, 1);
      else if (e.key === 'ArrowLeft') n = step(focusIdx, -1);
      else if (e.key === 'Home') n = edge(true);
      else if (e.key === 'End') n = edge(false);
      else if (e.key === 'Delete' && props.dismissable && props.onTabCloseRequest && !items[focusIdx].disabled) { e.preventDefault(); props.onTabCloseRequest(focusIdx); return; }
      if (n == null) return;
      e.preventDefault();
      if (manual) setFocus(n); else select(n);
      focusTab(n);
    }
    var navCls = cx('cds--tabs', props.contained && 'cds--tabs--contained', props.contained && props.fullWidth && 'cds--tabs--full-width', tall && 'cds--tabs--tall',
      props.dismissable && 'cds--tabs--dismissable', iconOnly && 'cds--tabs__icon--' + (props.iconSize === 'lg' ? 'lg' : 'default'));
    return h('div', { className: props.className },
      h('div', { className: navCls },
        h('div', { ref: navRef, className: 'cds--tabs__nav', role: 'tablist', 'aria-label': props.ariaLabel || props['aria-label'] || 'Tabs', onKeyDown: onKey },
          items.map(function (it, i) {
            var sel = i === idx;
            var closeBtn = props.dismissable ? h('span', {
              className: 'cds--tabs__nav-item--close-icon', 'aria-hidden': 'true', title: 'Close tab',
              onClick: function (e) { e.stopPropagation(); if (!it.disabled && props.onTabCloseRequest) props.onTabCloseRequest(i); }
            }, h(Icon, { name: 'close', size: 16 })) : null;
            var label = iconOnly
              ? h(Icon, { name: it.renderIcon, size: 16 })
              : h('span', { className: 'cds--tabs__nav-item-label-wrapper' },
                  h('span', { className: 'cds--tabs__nav-item-label' }, it.label),
                  it.renderIcon ? h(Icon, { name: it.renderIcon, size: 16, className: 'cds--tabs__nav-item--icon' }) : null);
            return h('div', { key: i, className: cx('cds--tabs__nav-item', sel && 'cds--tabs__nav-item--selected', it.disabled && 'cds--tabs__nav-item--disabled', iconOnly && 'cds--tabs__nav-item--icon-only') },
              h('button', {
                type: 'button', role: 'tab', id: autoId + '-t' + i, 'aria-selected': sel, 'aria-controls': autoId + '-p' + i,
                'aria-label': iconOnly ? it.label : undefined, title: iconOnly ? it.label : (typeof it.label === 'string' ? it.label : undefined),
                tabIndex: i === focusIdx ? 0 : -1, disabled: it.disabled, className: 'cds--tabs__nav-link', onClick: function () { select(i); },
                onFocus: function () { if (i !== focusIdx) setFocus(i); }
              }, label, it.secondaryLabel && !iconOnly ? h('span', { className: 'cds--tabs__nav-item-secondary-label' }, it.secondaryLabel) : null),
              closeBtn);
          }))),
      items.map(function (it, i) {
        return h('div', { key: i, role: 'tabpanel', id: autoId + '-p' + i, 'aria-labelledby': autoId + '-t' + i, tabIndex: 0, hidden: i !== idx, className: 'cds--tab-content' }, it.content);
      }));
  }

  var Carbon = window.Carbon = { Button: Button, Tag: Tag, TextInput: TextInput, Checkbox: Checkbox, Toggle: Toggle, InlineNotification: InlineNotification, Tile: Tile, Link: Link, Tabs: Tabs, Icon: Icon, ClickableTile: ClickableTile };

/* ---- A1 ---- */
(function(exports,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx){
/* @components: TextArea, NumberInput, PasswordInput, Search, Select, RadioButtonGroup, RadioButton, TimePicker, Slider */

// ---- shared helpers (local to this fragment) -------------------------------
function a1Id(id, prefix) { var auto = useId(); return id || (prefix + '-' + auto); }

// Invalid (circle with exclamation) and warning (triangle) glyphs, as in Carbon's useNormalizedInputProps.
function a1InvalidIcon(cls) { return h(Icon, { name: 'warning--filled', size: 16, className: cls }); }
// Carbon's WarningAltFilled: yellow triangle, dark "!" (inner path, coloured in styles.css).
var A1_WARN_ALT = '<path data-icon-path="inner-path" d="M16,26a1.5,1.5,0,1,1,1.5-1.5A1.5,1.5,0,0,1,16,26Zm-1.125-5h2.25V12h-2.25Z"/><path d="M16.002,6.1714h-.004L4.6487,27.9966,4.6506,28H27.3494l.0019-.0034ZM14.875,12h2.25v9h-2.25ZM16,26a1.5,1.5,0,1,1,1.5-1.5A1.5,1.5,0,0,1,16,26Z"/><path d="M29,30H3a1,1,0,0,1-.8872-1.4614l13-25a1,1,0,0,1,1.7744,0l13,25A1,1,0,0,1,29,30ZM4.6507,28H27.3493l.002-.0033L16.002,6.1714h-.004L4.6487,27.9967Z"/>';
function a1WarnIcon(cls) {
  return h('svg', {
    className: 'cds--icon ' + cls, width: 16, height: 16, viewBox: '0 0 32 32', focusable: 'false', 'aria-hidden': 'true',
    dangerouslySetInnerHTML: htmlOf(A1_WARN_ALT)
  });
}

// ---- TextArea --------------------------------------------------------------
function TextArea(props) {
  var id = a1Id(props.id, 'ta');
  var disabled = !!props.disabled, readOnly = !!props.readOnly;
  // As Carbon's useNormalizedInputProps: no invalid / warn styling on a disabled or read-only field.
  var invalid = !!props.invalid && !disabled && !readOnly, warn = !!props.warn && !invalid && !disabled && !readOnly;
  var maxCount = props.maxCount, mode = props.counterMode || 'character';
  var controlled = props.value !== undefined;
  var st = useState(String(props.defaultValue == null ? '' : props.defaultValue)); var inner = st[0], setInner = st[1];
  var text = controlled ? String(props.value == null ? '' : props.value) : inner;
  function count(s) { return mode === 'word' ? ((s.match(/\S+/g) || []).length) : s.length; }
  var n = count(text);
  function onChange(e) {
    if (disabled || readOnly) return;
    // As Carbon: in word mode, stop accepting input that would go over the limit (character mode uses maxLength).
    if (props.enableCounter && maxCount && mode === 'word' && count(e.target.value) > maxCount && count(e.target.value) > n) return;
    if (!controlled) setInner(e.target.value);
    if (props.onChange) props.onChange(e);
  }
  var showCounter = props.enableCounter && maxCount;
  var helperId = id + '-help', errId = id + '-err', warnId = id + '-warn', cntId = id + '-count';
  var describedBy = invalid ? errId : warn ? warnId : [props.helperText ? helperId : null, showCounter ? cntId : null].filter(Boolean).join(' ') || undefined;
  return h('div', { className: cx('cds--form-item', props.className) },
    h('div', { className: 'cds--text-area__label-wrapper' },
      props.labelText ? h('label', { htmlFor: id, className: cx('cds--label', props.hideLabel && 'cds--visually-hidden', disabled && 'cds--label--disabled') }, props.labelText) : null,
      showCounter ? h('div', { className: cx('cds--label', 'cds--text-area__label-counter', disabled && 'cds--label--disabled'), 'aria-hidden': 'true' }, n + '/' + maxCount) : null),
    showCounter ? h('span', { id: cntId, className: 'cds--visually-hidden' }, (mode === 'word' ? 'Word limit ' : 'Character limit ') + maxCount) : null,
    h('div', { className: cx('cds--text-area__wrapper', props.cols && 'cds--text-area__wrapper--cols', readOnly && 'cds--text-area__wrapper--readonly', warn && 'cds--text-area__wrapper--warn'), 'data-invalid': invalid || undefined },
      invalid ? a1InvalidIcon('cds--text-area__invalid-icon') : null,
      warn ? a1WarnIcon('cds--text-area__invalid-icon cds--text-area__invalid-icon--warning') : null,
      h('textarea', {
        id: id, className: cx('cds--text-area', invalid && 'cds--text-area--invalid', warn && 'cds--text-area--warn'),
        rows: props.rows || 4, cols: props.cols, placeholder: props.placeholder, disabled: disabled, readOnly: readOnly,
        maxLength: showCounter && mode === 'character' ? maxCount : undefined,
        value: controlled ? props.value : inner, onChange: onChange, onKeyDown: props.onKeyDown, onClick: props.onClick, onBlur: props.onBlur, onFocus: props.onFocus,
        name: props.name, required: props.required,
        'aria-invalid': invalid || undefined, 'aria-readonly': readOnly || undefined, 'aria-describedby': describedBy,
        style: props.cols ? undefined : { inlineSize: '100%' }
      })),
    !invalid && !warn && props.helperText ? h('div', { id: helperId, className: cx('cds--form__helper-text', disabled && 'cds--form__helper-text--disabled') }, props.helperText) : null,
    invalid ? h('div', { id: errId, role: 'alert', className: 'cds--form-requirement' }, props.invalidText) : null,
    warn ? h('div', { id: warnId, role: 'alert', className: 'cds--form-requirement cds--form-requirement--warn' }, props.warnText) : null);
}

// ---- NumberInput -----------------------------------------------------------
function a1Decimals(n) { var p = String(n).split('.'); return p[1] ? p[1].length : 0; }
function NumberInput(props) {
  var id = a1Id(props.id, 'num');
  var size = props.size || 'md', step = props.step == null ? 1 : props.step, min = props.min, max = props.max;
  var disabled = !!props.disabled, readOnly = !!props.readOnly;
  var controlled = props.value !== undefined;
  var st = useState(props.defaultValue !== undefined ? props.defaultValue : (props.allowEmpty ? '' : 0));
  var inner = st[0], setInner = st[1];
  var value = controlled ? props.value : inner;
  var num = (value === '' || value == null) ? NaN : Number(value);
  var outOfRange = !isNaN(num) && ((min != null && num < min) || (max != null && num > max));
  var invalid = (!!props.invalid || outOfRange || (isNaN(num) && !props.allowEmpty)) && !disabled && !readOnly;
  var warn = !invalid && !!props.warn && !disabled && !readOnly;
  function emit(e, v, dir) {
    if (!controlled) setInner(v);
    if (props.onChange) props.onChange(e, { value: v, direction: dir });
  }
  function onInput(e) {
    if (disabled || readOnly) return;
    var raw = e.target.value;
    var v = raw === '' ? (props.allowEmpty ? '' : raw) : Number(raw);
    var dir = !isNaN(Number(v)) && Number(v) < num ? 'down' : 'up';
    if (!controlled) setInner(raw === '' ? '' : v);
    if (props.onChange) props.onChange(e, { value: raw === '' && !props.allowEmpty ? 0 : v, direction: dir });
  }
  function stepBy(e, dir) {
    var cur = isNaN(num) ? 0 : num;
    var raw = dir === 'up' ? cur + step : cur - step;
    var prec = Math.max(a1Decimals(cur), a1Decimals(step));
    var v = parseFloat(Number(raw).toFixed(prec));
    if (min != null) v = Math.max(v, min);
    if (max != null) v = Math.min(v, max);
    emit(e, v, dir);
    if (props.onClick) props.onClick(e, { value: v, direction: dir });
  }
  var label = props.label || props.labelText;
  var helperId = id + '-help', msgId = id + '-msg';
  var describedBy = invalid || warn ? msgId : (props.helperText ? helperId : undefined);
  var incLabel = props.incrementNumLabel || 'Increment number', decLabel = props.decrementNumLabel || 'Decrement number';
  var stepper = props.hideSteppers ? null : h('div', { className: 'cds--number__controls' },
    h('button', { type: 'button', className: 'cds--number__control-btn down-icon', 'aria-label': decLabel, title: decLabel, tabIndex: -1, disabled: disabled || readOnly, onClick: function (e) { stepBy(e, 'down'); } },
      h('svg', { className: 'down-icon', width: 16, height: 16, viewBox: '0 0 16 16', 'aria-hidden': 'true', focusable: 'false', dangerouslySetInnerHTML: htmlOf('<path d="M2 7H14V9H2z"/>') })),
    h('div', { className: 'cds--number__rule-divider' }),
    h('button', { type: 'button', className: 'cds--number__control-btn up-icon', 'aria-label': incLabel, title: incLabel, tabIndex: -1, disabled: disabled || readOnly, onClick: function (e) { stepBy(e, 'up'); } },
      h('svg', { className: 'up-icon', width: 16, height: 16, viewBox: '0 0 16 16', 'aria-hidden': 'true', focusable: 'false', dangerouslySetInnerHTML: htmlOf('<path d="M9 7V2H7V7H2V9H7V14H9V9H14V7z"/>') })),
    h('div', { className: 'cds--number__rule-divider' }));
  return h('div', { className: cx('cds--form-item', props.className) },
    h('div', { className: cx('cds--number', 'cds--number--' + size, readOnly && 'cds--number--readonly', props.hideLabel && 'cds--number--nolabel', props.hideSteppers && 'cds--number--nosteppers'), 'data-invalid': invalid || undefined },
      label ? h('label', { htmlFor: id, className: cx('cds--label', props.hideLabel && 'cds--visually-hidden', disabled && 'cds--label--disabled') }, label) : null,
      h('div', { className: cx('cds--number__input-wrapper', warn && 'cds--number__input-wrapper--warning') },
        h('input', {
          id: id, type: 'number', inputMode: 'decimal', name: props.name, placeholder: props.placeholder,
          min: min, max: max, step: step, value: value == null ? '' : value, onChange: onInput, onBlur: props.onBlur, onKeyUp: props.onKeyUp, onKeyDown: props.onKeyDown,
          disabled: disabled, readOnly: readOnly, 'data-invalid': invalid || undefined,
          'aria-invalid': invalid || undefined, 'aria-readonly': readOnly || undefined, 'aria-describedby': describedBy
        }),
        invalid ? a1InvalidIcon('cds--number__invalid') : null,
        warn ? a1WarnIcon('cds--number__invalid cds--number__invalid--warning') : null,
        stepper),
      invalid ? h('div', { id: msgId, className: 'cds--form-requirement' }, props.invalidText || 'Number is not valid')
        : warn ? h('div', { id: msgId, className: 'cds--form-requirement cds--form-requirement--warn' }, props.warnText)
        : (props.helperText ? h('div', { id: helperId, className: cx('cds--form__helper-text', disabled && 'cds--form__helper-text--disabled') }, props.helperText) : null)));
}

// ---- PasswordInput ---------------------------------------------------------
function PasswordInput(props) {
  var id = a1Id(props.id, 'pw');
  var size = props.size || 'md';
  var disabled = !!props.disabled, readOnly = !!props.readOnly;
  var invalid = !!props.invalid && !disabled && !readOnly, warn = !invalid && !!props.warn && !disabled && !readOnly;
  var st = useState(props.type || 'password'); var type = st[0], setType = st[1];
  var visible = type === 'text';
  var hideLabel = props.hidePasswordLabel || 'Hide password', showLabel = props.showPasswordLabel || 'Show password';
  function toggle(e) { setType(visible ? 'password' : 'text'); if (props.onTogglePasswordVisibility) props.onTogglePasswordVisibility(e); }
  var helperId = id + '-help', msgId = id + '-msg';
  var msg = invalid ? h('div', { id: msgId, className: 'cds--form-requirement' }, props.invalidText)
    : warn ? h('div', { id: msgId, className: 'cds--form-requirement cds--form-requirement--warn' }, props.warnText)
    : (props.helperText ? h('div', { id: helperId, className: cx('cds--form__helper-text', disabled && 'cds--form__helper-text--disabled') }, props.helperText) : null);
  return h('div', { className: cx('cds--form-item', 'cds--text-input-wrapper', 'cds--password-input-wrapper', readOnly && 'cds--text-input-wrapper--readonly', props.className) },
    props.labelText ? h('label', { htmlFor: id, className: cx('cds--label', props.hideLabel && 'cds--visually-hidden', disabled && 'cds--label--disabled') }, props.labelText) : null,
    h('div', { className: 'cds--text-input__field-outer-wrapper' },
      h('div', { className: cx('cds--text-input__field-wrapper', warn && 'cds--text-input__field-wrapper--warning'), 'data-invalid': invalid || undefined },
        invalid ? a1InvalidIcon('cds--text-input__invalid-icon') : null,
        warn ? a1WarnIcon('cds--text-input__invalid-icon cds--text-input__invalid-icon--warning') : null,
        h('input', {
          id: id, type: type, className: cx('cds--text-input', 'cds--password-input', size !== 'md' && 'cds--text-input--' + size, invalid && 'cds--text-input--invalid', warn && 'cds--text-input--warning'),
          placeholder: props.placeholder, disabled: disabled, readOnly: readOnly, name: props.name, autoComplete: props.autoComplete,
          value: props.value, defaultValue: props.defaultValue, onChange: props.onChange, onBlur: props.onBlur, onFocus: props.onFocus,
          'aria-invalid': invalid || undefined, 'aria-describedby': (invalid || warn) ? msgId : (props.helperText ? helperId : undefined)
        }),
        h('div', { className: 'cds--toggle-password-tooltip' },
          h('button', { type: 'button', className: 'cds--text-input--password__visibility__toggle', disabled: disabled, onClick: toggle, 'aria-label': visible ? hideLabel : showLabel },
            h(Icon, { name: visible ? 'view--off' : 'view', size: 16 })),
          h('span', { className: 'cds--toggle-password-tooltip__content', role: 'tooltip', 'aria-hidden': 'true' }, visible ? hideLabel : showLabel))),
      msg));
}

// ---- Search ----------------------------------------------------------------
function Search(props) {
  var id = a1Id(props.id, 'search'); var searchId = id + '-search';
  var size = props.size || 'md', disabled = !!props.disabled, expandable = !!props.expandable;
  var inputRef = useRef(null), magRef = useRef(null), focusNext = useRef(false);
  var cs = useState(props.defaultValue == null ? '' : String(props.defaultValue)); var inner = cs[0], setInner = cs[1];
  var controlled = props.value !== undefined;
  var value = controlled ? String(props.value == null ? '' : props.value) : inner;
  var es = useState(!!props.defaultExpanded); var expState = es[0], setExp = es[1];
  var expanded = !expandable || (props.isExpanded !== undefined ? props.isExpanded : expState);
  var hasContent = value !== '';
  useEffect(function () { if (focusNext.current && expanded && inputRef.current) { inputRef.current.focus(); } focusNext.current = false; }, [expanded]);
  function expand(e) { if (disabled) return; focusNext.current = true; setExp(true); if (props.onExpand) props.onExpand(e); else if (inputRef.current && expanded) inputRef.current.focus(); }
  function collapse() { setExp(false); }
  function clear() {
    if (!controlled) setInner('');
    if (props.onChange) props.onChange({ type: 'change', target: { value: '', id: id }, currentTarget: { value: '' } });
    if (props.onClear) props.onClear();
    if (inputRef.current) inputRef.current.focus();
  }
  function onChange(e) { if (!controlled) setInner(e.target.value); if (props.onChange) props.onChange(e); }
  function onKeyDown(e) {
    if (props.onKeyDown) props.onKeyDown(e);
    if (e.key === 'Escape') {
      e.stopPropagation();
      if (inputRef.current && inputRef.current.value) clear();
      else if (expandable && expanded) { collapse(); if (magRef.current) magRef.current.focus(); }
    }
  }
  function onBlur(e) { if (expandable && !hasContent) collapse(); if (props.onBlur) props.onBlur(e); }
  function magKey(e) { if (expandable && !expanded && (e.key === 'Enter' || e.key === ' ')) { e.preventDefault(); expand(e); } }
  var placeholder = props.placeholder == null ? 'Search' : props.placeholder;
  var closeLabel = props.closeButtonLabelText || 'Clear search input';
  return h('div', { role: 'search', 'aria-label': placeholder, className: cx('cds--search', 'cds--search--' + size, expandable && 'cds--search--expandable', expandable && expanded && 'cds--search--expanded', disabled && 'cds--search--disabled', props.className) },
    h('div', {
      ref: magRef, className: 'cds--search-magnifier', role: expandable ? 'button' : undefined, tabIndex: expandable && !expanded && !disabled ? 0 : -1,
      'aria-labelledby': expandable ? searchId : undefined, 'aria-expanded': expandable ? expanded : undefined, 'aria-controls': expandable ? id : undefined,
      onClick: expandable && !expanded ? expand : undefined, onKeyDown: magKey
    }, h(Icon, { name: props.renderIcon || 'search', size: 16, className: 'cds--search-magnifier-icon' })),
    h('label', { id: searchId, htmlFor: id, className: 'cds--label' }, props.labelText),
    h('input', {
      ref: inputRef, id: id, type: props.type || 'search', className: 'cds--search-input', autoComplete: props.autoComplete || 'off',
      placeholder: placeholder, disabled: disabled, value: value, onChange: onChange, onKeyDown: onKeyDown, onBlur: onBlur, onFocus: props.onFocus,
      tabIndex: expandable && !expanded ? -1 : undefined, name: props.name
    }),
    h('button', { type: 'button', className: cx('cds--search-close', (!hasContent || !expanded) && 'cds--search-close--hidden'), 'aria-label': closeLabel, title: closeLabel, disabled: disabled, tabIndex: !hasContent || !expanded ? -1 : undefined, onClick: clear },
      h('svg', { className: 'cds--icon', width: 16, height: 16, viewBox: '0 0 16 16', focusable: 'false', 'aria-hidden': 'true', dangerouslySetInnerHTML: htmlOf('<path d="M12 4.7L11.3 4 8 7.3 4.7 4 4 4.7 7.3 8 4 11.3 4.7 12 8 8.7 11.3 12 12 11.3 8.7 8z"/>') })));
}
function ExpandableSearch(props) { return h(Search, Object.assign({}, props, { expandable: true })); }

// ---- Select ----------------------------------------------------------------
function SelectItem(props) {
  return h('option', { className: 'cds--select-option', value: props.value, disabled: props.disabled, hidden: props.hidden }, props.text);
}
function SelectItemGroup(props) {
  return h('optgroup', { className: 'cds--select-optgroup', label: props.label, disabled: props.disabled }, props.children);
}
function Select(props) {
  var id = a1Id(props.id, 'select');
  var size = props.size || 'md', disabled = !!props.disabled, readOnly = !!props.readOnly, inline = !!props.inline;
  var invalid = !!props.invalid && !readOnly && !disabled, warn = !invalid && !!props.warn && !readOnly && !disabled;
  var helperId = id + '-help', errId = id + '-err';
  var other = {};
  ['value', 'defaultValue', 'onChange', 'onBlur', 'onFocus', 'onMouseDown', 'onKeyDown', 'name', 'required', 'autoComplete', 'title'].forEach(function (k) { if (props[k] !== undefined) other[k] = props[k]; });
  var down = other.onMouseDown, key = other.onKeyDown;
  other.onMouseDown = function (e) { if (readOnly) { e.preventDefault(); e.target.focus(); } if (down) down(e); };
  other.onKeyDown = function (e) { if (readOnly && ['ArrowDown', 'ArrowUp', ' '].indexOf(e.key) > -1) e.preventDefault(); if (key) key(e); };
  var msg = invalid ? h('div', { id: errId, className: 'cds--form-requirement' }, props.invalidText)
    : warn ? h('div', { id: errId, className: 'cds--form-requirement cds--form-requirement--warn' }, props.warnText) : null;
  var helper = props.helperText ? h('div', { id: helperId, className: cx('cds--form__helper-text', disabled && 'cds--form__helper-text--disabled') }, props.helperText) : null;
  var describedBy = invalid || warn ? errId : (!inline && props.helperText ? helperId : undefined);
  var input = [
    h('select', Object.assign({ key: 's', id: id, className: cx('cds--select-input', 'cds--select-input--' + size), disabled: disabled || undefined, 'aria-invalid': invalid || undefined, 'aria-readonly': readOnly || undefined, 'aria-describedby': describedBy }, other), props.children),
    h(Icon, { key: 'a', name: 'chevron--down', size: 16, className: 'cds--select__arrow' }),
    invalid ? h(Icon, { key: 'i', name: 'warning--filled', size: 16, className: 'cds--select__invalid-icon' }) : null,
    warn ? a1WarnIcon('cds--select__invalid-icon cds--select__invalid-icon--warning') : null
  ];
  var label = props.noLabel ? null : h('label', { htmlFor: id, className: cx('cds--label', props.hideLabel && 'cds--visually-hidden', disabled && 'cds--label--disabled') }, props.labelText);
  return h('div', { className: cx('cds--form-item', props.className) },
    h('div', { className: cx('cds--select', inline && 'cds--select--inline', invalid && 'cds--select--invalid', disabled && 'cds--select--disabled', readOnly && 'cds--select--readonly', warn && 'cds--select--warning') },
      label,
      inline
        ? h('div', { className: 'cds--select-input--inline__wrapper', 'data-invalid': invalid || undefined },
            h('div', { className: 'cds--select-input__wrapper', 'data-invalid': invalid || undefined }, input), msg)
        : h('div', { className: 'cds--select-input__wrapper', 'data-invalid': invalid || undefined }, input),
      inline ? null : (msg || helper)));
}

// ---- RadioButton / RadioButtonGroup ----------------------------------------
function RadioButton(props) {
  var id = a1Id(props.id, 'radio');
  var pos = props.labelPosition || 'right';
  function onChange(e) { if (props.onChange) props.onChange(props.value === undefined ? '' : props.value, props.name, e); }
  var inputProps = {
    type: 'radio', id: id, className: 'cds--radio-button', onChange: onChange, disabled: props.disabled, value: props.value === undefined ? '' : props.value,
    name: props.name, required: props.required, readOnly: props.readOnly, 'aria-readonly': props.readOnly || undefined
  };
  if (props.checked !== undefined) inputProps.checked = props.checked;
  if (props.defaultChecked !== undefined) inputProps.defaultChecked = props.defaultChecked;
  return h('div', { className: cx(props.className, 'cds--radio-button-wrapper', pos !== 'right' && 'cds--radio-button-wrapper--label-' + pos) },
    h('input', inputProps),
    h('label', { htmlFor: id, className: 'cds--radio-button__label' },
      h('span', { className: 'cds--radio-button__appearance' }),
      props.labelText ? h('span', { className: cx('cds--radio-button__label-text', props.hideLabel && 'cds--visually-hidden') }, props.labelText) : null));
}
function RadioButtonGroup(props) {
  var orientation = props.orientation || 'horizontal', pos = props.labelPosition || 'right';
  var readOnly = !!props.readOnly, disabled = !!props.disabled;
  var controlled = props.valueSelected !== undefined;
  var st = useState(props.defaultSelected); var inner = st[0], setInner = st[1];
  var selected = controlled ? props.valueSelected : inner;
  var autoId = useId();
  var invalid = !readOnly && !disabled && !!props.invalid;
  var warn = !readOnly && !disabled && !invalid && !!props.warn;
  var showHelper = !invalid && !disabled && !warn;
  var helperId = props.helperText ? 'rbg-help-' + autoId : undefined, msgId = 'rbg-msg-' + autoId;
  function onChange(v, name, e) {
    if (readOnly) return;
    if (v !== selected) { if (!controlled) setInner(v); if (props.onChange) props.onChange(v, name, e); }
  }
  var kids = React.Children.map(props.children, function (child) {
    if (!child) return null;
    return React.cloneElement(child, { name: props.name, key: child.props.value, value: child.props.value, onChange: onChange, checked: child.props.value === selected, required: props.required });
  });
  return h('div', { className: cx('cds--form-item', props.className) },
    h('fieldset', {
      className: cx('cds--radio-button-group', orientation === 'vertical' && 'cds--radio-button-group--vertical', 'cds--radio-button-group--label-' + pos, readOnly && 'cds--radio-button-group--readonly', invalid && 'cds--radio-button-group--invalid', warn && 'cds--radio-button-group--warning'),
      disabled: disabled, 'data-invalid': invalid ? true : undefined, 'aria-readonly': readOnly || undefined,
      'aria-describedby': invalid || warn ? msgId : (showHelper ? helperId : undefined)
    },
      props.legendText ? h('legend', { className: 'cds--label' }, props.legendText) : null,
      kids),
    h('div', { className: 'cds--radio-button__validation-msg' },
      invalid ? h(React.Fragment, null, a1InvalidIcon('cds--radio-button__invalid-icon'), h('div', { id: msgId, className: 'cds--form-requirement' }, props.invalidText)) : null,
      warn ? h(React.Fragment, null, a1WarnIcon('cds--radio-button__invalid-icon cds--radio-button__invalid-icon--warning'), h('div', { id: msgId, className: 'cds--form-requirement cds--form-requirement--warn' }, props.warnText)) : null),
    showHelper && props.helperText ? h('div', { id: helperId, className: cx('cds--form__helper-text', disabled && 'cds--form__helper-text--disabled') }, props.helperText) : null);
}

// ---- TimePicker ------------------------------------------------------------
function TimePickerSelect(props) {
  return h(Select, Object.assign({}, props, { className: cx('cds--time-picker__select', props.className), labelText: props.labelText || 'Provide label text', hideLabel: true }));
}
function TimePicker(props) {
  var id = a1Id(props.id, 'tp');
  var size = props.size || 'md', disabled = !!props.disabled, readOnly = !!props.readOnly;
  var invalid = !!props.invalid && !readOnly && !disabled, warn = !invalid && !!props.warning && !readOnly && !disabled;
  var msgId = id + '-msg';
  var kids = React.Children.map(props.children, function (c) {
    if (!c) return null;
    return React.cloneElement(c, { disabled: c.props.disabled != null ? c.props.disabled : disabled, readOnly: readOnly });
  });
  return h('div', { className: cx('cds--form-item', props.className) },
    props.labelText ? h('label', { htmlFor: id, className: cx('cds--label', props.hideLabel && 'cds--visually-hidden', disabled && 'cds--label--disabled') }, props.labelText) : null,
    h('div', { className: cx('cds--time-picker', invalid && 'cds--time-picker--invalid', warn && 'cds--time-picker--warning', readOnly && 'cds--time-picker--readonly', 'cds--time-picker--' + size, props.pickerClassName) },
      h('div', { className: 'cds--time-picker__input' },
        h('input', {
          id: id, type: props.type || 'text', className: cx('cds--time-picker__input-field', 'cds--text-input', props.inputClassName, (invalid || warn) && 'cds--time-picker__input-field-error'),
          maxLength: props.maxLength == null ? 5 : props.maxLength, placeholder: props.placeholder == null ? 'hh:mm' : props.placeholder,
          pattern: props.pattern || '(1[012]|[1-9]):[0-5][0-9](\\s)?', disabled: disabled, readOnly: readOnly, name: props.name,
          value: props.value, defaultValue: props.defaultValue, onChange: props.onChange, onBlur: props.onBlur, onClick: props.onClick, onFocus: props.onFocus,
          'data-invalid': invalid || undefined, 'aria-invalid': invalid || undefined, 'aria-describedby': invalid || warn ? msgId : undefined
        }),
        invalid ? h('div', { className: 'cds--time-picker__error__icon' }, a1InvalidIcon('cds--checkbox__invalid-icon')) : null,
        warn ? h('div', { className: 'cds--time-picker__error__icon' }, a1WarnIcon('cds--text-input__invalid-icon--warning')) : null),
      kids),
    invalid ? h('div', { id: msgId, className: 'cds--form-requirement' }, props.invalidText == null ? 'Invalid time format.' : props.invalidText)
      : warn ? h('div', { id: msgId, className: 'cds--form-requirement cds--form-requirement--warn' }, props.warningText == null ? 'Warning message.' : props.warningText) : null);
}

// ---- Slider ----------------------------------------------------------------
var A1_LOWER = '<path d="M15.08 6.46H16v11.08h-.92zM4.46 17.54c-.25 0-.46-.21-.46-.46V6.92a.465.465 0 0 1 .69-.4l8.77 5.08a.46.46 0 0 1 0 .8l-8.77 5.08c-.07.04-.15.06-.23.06Z"/>';
var A1_LOWER_F = A1_LOWER + '<path d="M15.08 0H16v6.46h-.92z"/><path d="M0 0h.92v24H0zM15.08 0H16v24h-.92z"/><path d="M0 .92V0h16v.92zM0 24v-.92h16V24z"/>';
var A1_UPPER = '<path d="M0 6.46h.92v11.08H0zM11.54 6.46c.25 0 .46.21.46.46v10.15a.465.465 0 0 1-.69.4L2.54 12.4a.46.46 0 0 1 0-.8l8.77-5.08c.07-.04.15-.06.23-.06Z"/>';
var A1_UPPER_F = A1_UPPER + '<path d="M.92 24H0v-6.46h.92z"/><path d="M16 24h-.92V0H16zM.92 24H0V0h.92z"/><path d="M16 23.08V24H0v-.92zM16 0v.92H0V0z"/>';
function a1Handle(cls, body) {
  return h('svg', { className: 'cds--slider__thumb-icon ' + cls, viewBox: '0 0 16 24', width: 16, height: 24, 'aria-hidden': 'true', focusable: 'false', dangerouslySetInnerHTML: htmlOf(body) });
}
function Slider(props) {
  var id = a1Id(props.id, 'slider');
  var min = props.min == null ? 0 : props.min, max = props.max == null ? 100 : props.max, step = props.step == null ? 1 : props.step;
  var mult = props.stepMultiplier || 4, two = !!props.twoHandles, disabled = !!props.disabled, readOnly = !!props.readOnly;
  var formatLabel = props.formatLabel || function (v, l) { return v + (l || ''); };
  var init = { lo: props.value !== undefined ? props.value : (props.defaultValue !== undefined ? props.defaultValue : min), hi: props.valueUpper !== undefined ? props.valueUpper : (props.defaultValueUpper !== undefined ? props.defaultValueUpper : max) };
  var st = useState(init); var s = st[0], setS = st[1];
  var ds = useState(null); var draft = ds[0], setDraft = ds[1];
  var sRef = useRef(s); sRef.current = s;
  var trackRef = useRef(null), loRef = useRef(null), hiRef = useRef(null), active = useRef('lo');
  useEffect(function () {
    if (props.value !== undefined || props.valueUpper !== undefined) {
      setS(function (p) { return { lo: props.value !== undefined ? props.value : p.lo, hi: props.valueUpper !== undefined ? props.valueUpper : p.hi }; });
    }
  }, [props.value, props.valueUpper]);
  var prec = Math.max(a1Decimals(step), a1Decimals(min));
  function clamp(v) { return Math.min(max, Math.max(min, v)); }
  function snap(v) {
    var r = min + Math.round((v - min) / step) * step;
    return clamp(parseFloat(r.toFixed(prec)));
  }
  function emit(n) { if (props.onChange) props.onChange(two ? { value: n.lo, valueUpper: n.hi } : { value: n.lo }); }
  function setHandle(which, v) {
    var cur = sRef.current, n = { lo: cur.lo, hi: cur.hi };
    if (!two) n.lo = v;
    else if (which === 'lo') n.lo = Math.min(v, cur.hi);
    else n.hi = Math.max(v, cur.lo);
    if (n.lo === cur.lo && n.hi === cur.hi) return;
    sRef.current = n; setS(n); emit(n);
  }
  function pctOf(v) { return max === min ? 0 : Math.min(100, Math.max(0, ((v - min) / (max - min)) * 100)); }
  function fromX(x) {
    var r = trackRef.current.getBoundingClientRect(), w = r.width > 0 ? r.width : 1;
    return snap(min + Math.min(1, Math.max(0, (x - r.left) / w)) * (max - min));
  }
  function xOf(e) { return e.touches && e.touches.length ? e.touches[0].clientX : e.clientX; }
  function onDown(e) {
    if (disabled || readOnly) return;
    if (e.button !== undefined && e.button !== 0) return;
    var v = fromX(xOf(e)), cur = sRef.current, which = 'lo';
    if (two) which = (Math.abs(v - cur.lo) < Math.abs(v - cur.hi) || (v === cur.lo && v === cur.hi && v < min + (max - min) / 2)) ? 'lo' : 'hi';
    active.current = which; setHandle(which, v);
    var el = which === 'lo' ? loRef.current : hiRef.current; if (el) el.focus();
    function move(ev) { setHandle(active.current, fromX(xOf(ev))); }
    function up() {
      document.removeEventListener('mousemove', move); document.removeEventListener('mouseup', up);
      document.removeEventListener('touchmove', move); document.removeEventListener('touchend', up);
    }
    document.addEventListener('mousemove', move); document.addEventListener('mouseup', up);
    document.addEventListener('touchmove', move); document.addEventListener('touchend', up);
  }
  function onKey(which) {
    return function (e) {
      if (disabled || readOnly) return;
      var cur = sRef.current[which], v = null, k = e.key;
      var d = (k === 'ArrowRight' || k === 'ArrowUp') ? step : (k === 'ArrowLeft' || k === 'ArrowDown') ? -step : 0;
      if (d) { v = snap(cur + (e.shiftKey ? d * mult : d)); }
      else if (k === 'Home') v = min; else if (k === 'End') v = max;
      else if (k === 'PageUp') v = snap(cur + step * mult); else if (k === 'PageDown') v = snap(cur - step * mult);
      if (v === null) return;
      e.preventDefault(); active.current = which; setHandle(which, v);
    };
  }
  function draftInvalid(w) { if (!draft || draft.which !== w) return false; var n = parseFloat(draft.text); return isNaN(n) || n < min || n > max; }
  // As the other fields: no invalid / warn styling while disabled or read-only.
  var off = disabled || readOnly;
  var invalidLo = !off && (!!props.invalid || draftInvalid('lo')), invalidHi = !off && (!!props.invalid || draftInvalid('hi'));
  var warnAny = !off && !!props.warn && !invalidLo && !invalidHi;
  function textInput(which) {
    var val = which === 'lo' ? s.lo : s.hi;
    var bad = which === 'lo' ? invalidLo : invalidHi;
    var shown = draft && draft.which === which ? draft.text : String(val);
    function commit() {
      if (!draft || draft.which !== which) return;
      var n = parseFloat(draft.text);
      if (!isNaN(n)) setHandle(which, clamp(n));
      setDraft(null);
    }
    return h('div', { className: cx('cds--slider-text-input-wrapper', 'cds--slider-text-input-wrapper--' + (which === 'lo' ? 'lower' : 'upper'), props.hideTextInput && 'cds--slider-text-input-wrapper--hidden') },
      h('input', {
        type: 'number', id: id + '-' + (which === 'lo' && two ? '' : (two ? 'upper-' : '')) + 'input-for-slider',
        className: cx('cds--text-input', 'cds--slider-text-input', bad && 'cds--text-input--invalid', warnAny && 'cds--slider-text-input--warn'),
        name: which === 'hi' && two ? props.nameUpper : props.name, min: min, max: max, step: step, value: shown, disabled: disabled, readOnly: readOnly,
        'aria-label': two ? (which === 'lo' ? (props.ariaLabelInput || 'Lower bound') : (props.ariaLabelInputUpper || 'Upper bound')) : props.ariaLabelInput,
        'aria-labelledby': !two && !props.ariaLabelInput ? id + '-label' : undefined,
        'aria-invalid': bad || undefined, 'data-invalid': bad || undefined,
        onChange: function (e) {
          if (disabled || readOnly) return;
          var t = e.target.value, n = parseFloat(t);
          setDraft({ which: which, text: t });
          if (!isNaN(n) && n >= min && n <= max) setHandle(which, n);
        },
        onBlur: commit, onKeyDown: function (e) { if (e.key === 'Enter') commit(); }
      }),
      bad ? a1InvalidIcon('cds--slider__invalid-icon') : null,
      warnAny && (which === 'hi' || !two) ? a1WarnIcon('cds--slider__invalid-icon cds--slider__invalid-icon--warning') : null);
  }
  function thumb(which) {
    var lo = which === 'lo', val = lo ? s.lo : s.hi;
    var handle = two ? (lo ? [a1Handle('cds--slider__thumb-icon--lower', A1_LOWER), a1Handle('cds--slider__thumb-icon--lower cds--slider__thumb-icon--focus', A1_LOWER_F)]
      : [a1Handle('cds--slider__thumb-icon--upper', A1_UPPER), a1Handle('cds--slider__thumb-icon--upper cds--slider__thumb-icon--focus', A1_UPPER_F)]) : null;
    return h('div', { className: cx('cds--slider__thumb-wrapper', two && (lo ? 'cds--slider__thumb-wrapper--lower' : 'cds--slider__thumb-wrapper--upper')), style: { insetInlineStart: pctOf(val) + '%' } },
      h('div', {
        ref: lo ? loRef : hiRef, role: 'slider', tabIndex: readOnly || disabled ? undefined : 0,
        className: cx('cds--slider__thumb', two && (lo ? 'cds--slider__thumb--lower' : 'cds--slider__thumb--upper')),
        'aria-valuemin': two && !lo ? s.lo : min, 'aria-valuemax': two && lo ? s.hi : max, 'aria-valuenow': val, 'aria-valuetext': formatLabel(val, undefined),
        'aria-labelledby': two ? undefined : id + '-label', 'aria-label': two ? (lo ? (props.ariaLabelInput || 'Lower bound') : (props.ariaLabelInputUpper || 'Upper bound')) : undefined,
        'aria-disabled': disabled || undefined, 'aria-readonly': readOnly || undefined, onKeyDown: onKey(which)
      }, handle));
  }
  var filled = two ? { transform: 'translate(' + pctOf(s.lo) + '%, -50%) scaleX(' + ((pctOf(s.hi) - pctOf(s.lo)) / 100) + ')' } : { transform: 'translate(0%, -50%) scaleX(' + (pctOf(s.lo) / 100) + ')' };
  var anyInvalid = invalidLo || invalidHi;
  return h('div', { className: cx('cds--form-item', props.className) },
    h('label', { id: id + '-label', htmlFor: two ? undefined : id + '-input-for-slider', className: cx('cds--label', props.hideLabel && 'cds--visually-hidden', disabled && 'cds--label--disabled') }, props.labelText),
    h('div', { className: cx('cds--slider-container', two && 'cds--slider-container--two-handles', disabled && 'cds--slider-container--disabled', readOnly && 'cds--slider-container--readonly') },
      two ? textInput('lo') : null,
      h('span', { className: 'cds--slider__range-label' }, formatLabel(min, props.minLabel)),
      h('div', { ref: trackRef, className: cx('cds--slider', disabled && 'cds--slider--disabled', readOnly && 'cds--slider--readonly'), role: 'presentation', onMouseDown: onDown, onTouchStart: onDown, 'data-invalid': anyInvalid || undefined },
        thumb('lo'), two ? thumb('hi') : null,
        h('div', { className: 'cds--slider__track' }),
        h('div', { className: 'cds--slider__filled-track', style: filled })),
      h('span', { className: 'cds--slider__range-label' }, formatLabel(max, props.maxLabel)),
      textInput(two ? 'hi' : 'lo')),
    anyInvalid ? h('div', { role: 'alert', className: 'cds--slider__validation-msg cds--slider__validation-msg--invalid cds--form-requirement' }, props.invalidText || 'Value is out of range')
      : warnAny ? h('div', { className: 'cds--slider__validation-msg cds--form-requirement cds--form-requirement--warn' }, props.warnText) : null);
}

// ---- Skeletons (loading placeholders, as Carbon's *Skeleton components) ----
function a1SkLabel(props) { return props.hideLabel ? null : h('span', { className: 'cds--label cds--skeleton' }); }
function TextAreaSkeleton(props) {
  return h('div', { className: cx('cds--form-item', props.className) }, a1SkLabel(props), h('div', { className: 'cds--skeleton cds--text-area' }));
}
function NumberInputSkeleton(props) {
  return h('div', { className: cx('cds--form-item', props.className) }, a1SkLabel(props), h('div', { className: 'cds--number cds--skeleton' }));
}
function SearchSkeleton(props) {
  var size = props.size || (props.small ? 'sm' : 'md');
  return h('div', { className: cx('cds--search', 'cds--search--' + size, 'cds--skeleton', props.className) },
    h('span', { className: 'cds--label' }), h('div', { className: 'cds--search-input' }));
}
function SelectSkeleton(props) {
  return h('div', { className: cx('cds--form-item', props.className) }, a1SkLabel(props),
    h('div', { className: 'cds--select cds--skeleton' }, h('div', { className: 'cds--select-input' })));
}
function RadioButtonSkeleton(props) {
  return h('div', { className: cx('cds--radio-button-wrapper', 'cds--radio-button-wrapper--skeleton', props.className) },
    h('span', { className: 'cds--radio-button__appearance cds--skeleton' }),
    h('span', { className: 'cds--radio-button__label cds--skeleton' }));
}
function SliderSkeleton(props) {
  var two = !!props.twoHandles;
  var thumb = function (pos, extra) {
    return h('div', { className: cx('cds--slider__thumb-wrapper', extra), style: { insetInlineStart: pos } }, h('div', { className: 'cds--slider__thumb' }));
  };
  return h('div', { className: cx('cds--form-item', props.className) }, a1SkLabel(props),
    h('div', { className: cx('cds--slider-container', 'cds--skeleton', two && 'cds--slider-container--two-handles') },
      h('span', { className: 'cds--slider__range-label' }),
      h('div', { className: 'cds--slider cds--skeleton__slider' },
        two ? thumb('25%') : thumb('50%'), two ? thumb('75%') : null,
        h('div', { className: 'cds--slider__track' })),
      h('span', { className: 'cds--slider__range-label' })));
}

exports.TextArea = TextArea;
exports.NumberInput = NumberInput;
exports.PasswordInput = PasswordInput;
exports.Search = Search;
exports.ExpandableSearch = ExpandableSearch;
exports.Select = Select;
exports.SelectItem = SelectItem;
exports.SelectItemGroup = SelectItemGroup;
exports.RadioButtonGroup = RadioButtonGroup;
exports.RadioButton = RadioButton;
exports.TimePicker = TimePicker;
exports.TimePickerSelect = TimePickerSelect;
exports.Slider = Slider;
exports.TextAreaSkeleton = TextAreaSkeleton;
exports.NumberInputSkeleton = NumberInputSkeleton;
exports.SearchSkeleton = SearchSkeleton;
exports.SelectSkeleton = SelectSkeleton;
exports.RadioButtonSkeleton = RadioButtonSkeleton;
exports.SliderSkeleton = SliderSkeleton;

})(Carbon,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx);

/* ---- A2 ---- */
(function(exports,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx){
/* @components: Dropdown, ComboBox, MultiSelect, FileUploader, DatePicker */
var Fragment = React.Fragment;

/* ===================================================================== */
/* Shared list-box helpers (Dropdown, ComboBox, MultiSelect)             */
/* ===================================================================== */
function lbText(item) {
  if (item == null) return '';
  if (typeof item === 'string') return item;
  if (typeof item === 'number') return String(item);
  if (item.label != null) return String(item.label);
  if (item.text != null) return String(item.text);
  if (item.name != null) return String(item.name);
  return String(item);
}
function lbDisabled(item) { return !!(item && typeof item === 'object' && item.disabled); }
function lbSame(a, b) {
  if (a === b) return true;
  if (a && b && typeof a === 'object' && typeof b === 'object' && a.id !== undefined && a.id === b.id) return true;
  return false;
}
function lbIndexOf(list, item) {
  for (var i = 0; i < list.length; i++) { if (lbSame(list[i], item)) return i; }
  return -1;
}
function lbNext(items, from, dir) {
  var n = items.length, i = from;
  for (var c = 0; c < n; c++) {
    i = i < 0 ? (dir > 0 ? 0 : n - 1) : (i + dir + n) % n;
    if (!lbDisabled(items[i])) return i;
  }
  return -1;
}
function lbEdge(items, last) {
  var n = items.length;
  for (var c = 0; c < n; c++) {
    var i = last ? n - 1 - c : c;
    if (!lbDisabled(items[i])) return i;
  }
  return -1;
}
function lbScroll(menu, idx) {
  if (!menu || idx < 0) return;
  var el = menu.children[idx]; if (!el) return;
  var t = el.offsetTop, b = t + el.offsetHeight;
  if (t < menu.scrollTop) menu.scrollTop = t;
  else if (b > menu.scrollTop + menu.clientHeight) menu.scrollTop = b - menu.clientHeight;
}
function lbOpenState(props) {
  var st = useState(!!props.defaultOpen);
  var ctl = props.open !== undefined;
  var open = ctl ? !!props.open : st[0];
  function setOpen(v) { if (!ctl) st[1](v); if (props.onOpenChange) props.onOpenChange(v); }
  return [open && !props.disabled, setOpen];
}
function lbOutside(ref, open, onOut) {
  var cb = useRef(onOut); cb.current = onOut;
  useEffect(function () {
    if (!open) return undefined;
    function f(e) { if (ref.current && !ref.current.contains(e.target)) cb.current(e); }
    document.addEventListener('mousedown', f);
    return function () { document.removeEventListener('mousedown', f); };
  }, [open]);
}
function noSpace(e) { if (e.key === ' ') e.preventDefault(); }
function lbMenuIcon(open) {
  return h('div', { className: cx('cds--list-box__menu-icon', open && 'cds--list-box__menu-icon--open') },
    h(Icon, { name: 'chevron--down', size: 16, title: open ? 'Close menu' : 'Open menu' }));
}
// Carbon's WarningAltFilled: yellow triangle, dark "!" (inner path, coloured in styles.css).
var A2_WARN_ALT = '<path data-icon-path="inner-path" d="M16,26a1.5,1.5,0,1,1,1.5-1.5A1.5,1.5,0,0,1,16,26Zm-1.125-5h2.25V12h-2.25Z"/><path d="M16.002,6.1714h-.004L4.6487,27.9966,4.6506,28H27.3494l.0019-.0034ZM14.875,12h2.25v9h-2.25ZM16,26a1.5,1.5,0,1,1,1.5-1.5A1.5,1.5,0,0,1,16,26Z"/><path d="M29,30H3a1,1,0,0,1-.8872-1.4614l13-25a1,1,0,0,1,1.7744,0l13,25A1,1,0,0,1,29,30ZM4.6507,28H27.3493l.002-.0033L16.002,6.1714h-.004L4.6487,27.9967Z"/>';
function a2WarnIcon(cls) {
  return h('svg', { className: 'cds--icon ' + cls, width: 16, height: 16, viewBox: '0 0 32 32', focusable: 'false', 'aria-hidden': 'true', dangerouslySetInnerHTML: htmlOf(A2_WARN_ALT) });
}
function lbInvalidIcons(invalid, warn) {
  return [
    invalid ? h(Icon, { key: 'i', name: 'warning--filled', size: 16, className: 'cds--list-box__invalid-icon' }) : null,
    (!invalid && warn) ? h(React.Fragment, { key: 'w' }, a2WarnIcon('cds--list-box__invalid-icon cds--list-box__invalid-icon--warning')) : null
  ];
}
// The shared list-box shell: the box itself plus its validation message.
function lbShell(o, kids) {
  var showWarn = !o.invalid && o.warn;
  var cls = cx('cds--list-box', o.size && 'cds--list-box--' + o.size, o.type === 'inline' && 'cds--list-box--inline', o.disabled && 'cds--list-box--disabled', o.open && 'cds--list-box--expanded', o.invalid && 'cds--list-box--invalid', showWarn && 'cds--list-box--warning', o.up && 'cds--list-box--up', o.cls);
  return h(Fragment, null,
    h('div', { ref: o.boxRef, id: o.id, className: cls, 'data-invalid': o.invalid || undefined, onKeyDown: o.onKeyDown }, kids),
    o.invalid ? h('div', { id: o.invalidId, className: 'cds--form-requirement' }, o.invalidText) : null,
    showWarn ? h('div', { id: o.warnId, className: 'cds--form-requirement' }, o.warnText) : null);
}
function lbWrapper(props, cls, kids) {
  var inline = props.type === 'inline';
  return h('div', { className: cx(cls, 'cds--list-box__wrapper', inline && 'cds--list-box__wrapper--inline', props.className) }, kids);
}
function lbLabel(props, id, labelId) {
  if (!props.titleText) return null;
  return h('label', { id: labelId, htmlFor: id, className: cx('cds--label', props.disabled && 'cds--label--disabled', props.hideLabel && 'cds--visually-hidden') }, props.titleText);
}
function lbHelper(props, helperId) {
  var n = lbNorm(props);
  if (props.type === 'inline' || n.invalid || n.warn || !props.helperText) return null;
  return h('div', { id: helperId, className: cx('cds--form__helper-text', props.disabled && 'cds--form__helper-text--disabled') }, props.helperText);
}
// Carbon's useNormalizedInputProps: a disabled or read-only field never shows invalid / warn.
function lbNorm(props) {
  var off = !!props.disabled || !!props.readOnly;
  var invalid = !!props.invalid && !off;
  return { invalid: invalid, warn: !!props.warn && !invalid && !off };
}
function lbSelectedIcon() { return h(Icon, { name: 'checkmark', size: 16, className: 'cds--list-box__menu-item__selected-icon' }); }

/* ===================================================================== */
/* Dropdown                                                              */
/* ===================================================================== */
function Dropdown(props) {
  var autoId = useId(); var id = props.id || ('dd-' + autoId);
  var labelId = id + '-label', menuId = id + '-menu', helperId = id + '-help', invalidId = id + '-err', warnId = id + '-warn';
  var items = props.items || [];
  var toStr = props.itemToString || lbText;
  var st = useState(props.initialSelectedItem !== undefined ? props.initialSelectedItem : null);
  var ctl = props.selectedItem !== undefined;
  var selected = ctl ? props.selectedItem : st[0];
  var os = lbOpenState(props), open = os[0], setOpen = os[1];
  var hs = useState(-1), hi = hs[0], setHi = hs[1];
  var menuRef = useRef(null), boxRef = useRef(null), kbd = useRef(0), typed = useRef({ s: '', t: 0 });
  var readOnly = props.readOnly, disabled = props.disabled;
  var nv = lbNorm(props), invalid = nv.invalid, warn = nv.warn;
  var selIdx = lbIndexOf(items, selected);

  lbOutside(boxRef, open, function () { setOpen(false); setHi(-1); });
  useEffect(function () { if (open) lbScroll(menuRef.current, hi); }, [hi, open]);
  useEffect(function () { if (open && hi < 0) lbScroll(menuRef.current, selIdx); }, [open]);

  function choose(item) {
    if (lbDisabled(item)) return;
    if (!ctl) st[1](item);
    if (props.onChange) props.onChange({ selectedItem: item });
    setOpen(false); setHi(-1);
  }
  function openWith(i) { setOpen(true); setHi(i); }
  function onKeyDown(e) {
    if (disabled || readOnly) return;
    var k = e.key;
    if (k === 'ArrowDown' || k === 'ArrowUp') {
      e.preventDefault();
      var dir = k === 'ArrowDown' ? 1 : -1;
      if (!open) openWith(selIdx >= 0 ? selIdx : lbNext(items, -1, dir));
      else setHi(lbNext(items, hi, dir));
    } else if (k === 'Home' && open) { e.preventDefault(); setHi(lbEdge(items, false)); }
    else if (k === 'End' && open) { e.preventDefault(); setHi(lbEdge(items, true)); }
    else if (k === 'Enter' || k === ' ') {
      e.preventDefault();
      if (open) { if (hi >= 0) choose(items[hi]); else setOpen(false); }
      else openWith(selIdx >= 0 ? selIdx : -1);
    } else if (k === 'Escape') {
      if (open) { e.preventDefault(); e.stopPropagation(); setOpen(false); setHi(-1); }
    } else if (k === 'Tab') { if (open) { setOpen(false); setHi(-1); } }
    else if (k.length === 1 && !e.ctrlKey && !e.metaKey && !e.altKey) {
      var now = Date.now(), t = typed.current;
      t.s = (now - t.t < 600 ? t.s : '') + k.toLowerCase(); t.t = now;
      for (var c = 0; c < items.length; c++) {
        var j = (Math.max(hi, selIdx) + 1 + c) % items.length;
        if (!lbDisabled(items[j]) && toStr(items[j]).toLowerCase().indexOf(t.s) === 0) { if (!open) setOpen(true); setHi(j); break; }
      }
    }
  }
  function onClick() {
    if (disabled || readOnly) return;
    if (open) { setOpen(false); setHi(-1); } else setOpen(true);
  }
  var describedBy = invalid ? invalidId : warn ? warnId : (props.type !== 'inline' && props.helperText ? helperId : undefined);
  var labelText = selected != null ? toStr(selected) : props.label;
  var Render = props.itemToElement;
  var kids = [
    lbInvalidIcons(invalid, warn),
    h('button', {
      key: 'f', id: id, type: 'button', className: 'cds--list-box__field', disabled: disabled, role: 'combobox',
      'aria-haspopup': 'listbox', 'aria-expanded': open, 'aria-controls': menuId, 'aria-disabled': readOnly ? true : undefined,
      'aria-labelledby': props.titleText ? labelId : undefined, 'aria-label': props.titleText ? undefined : (props['aria-label'] || props.label),
      'aria-describedby': describedBy, 'aria-activedescendant': open && hi >= 0 ? id + '-item-' + hi : undefined,
      title: labelText, onClick: onClick, onKeyDown: onKeyDown, onKeyUp: noSpace
    },
      h('span', { className: 'cds--list-box__label' }, labelText),
      lbMenuIcon(open)),
    h('ul', { key: 'm', ref: menuRef, id: menuId, className: 'cds--list-box__menu', role: 'listbox', 'aria-labelledby': props.titleText ? labelId : undefined },
      open ? items.map(function (item, i) {
        var dis = lbDisabled(item);
        return h('li', {
          key: i, id: id + '-item-' + i, role: 'option', 'aria-selected': lbSame(item, selected), 'aria-disabled': dis || undefined, disabled: dis || undefined,
          className: cx('cds--list-box__menu-item', lbSame(item, selected) && 'cds--list-box__menu-item--active', hi === i && 'cds--list-box__menu-item--highlighted'),
          onMouseDown: function (e) { e.preventDefault(); }, onClick: function () { choose(item); }
        },
          h('div', { className: 'cds--list-box__menu-item__option', title: toStr(item) },
            Render && typeof item === 'object' ? h(Render, item) : toStr(item),
            lbSame(item, selected) ? lbSelectedIcon() : null));
      }) : null)
  ];
  var inline = props.type === 'inline';
  var sz = props.size && props.size !== 'md' ? props.size : undefined;
  return lbWrapper(props, 'cds--dropdown__wrapper' + (inline ? ' cds--dropdown__wrapper--inline' : ''),
    h(Fragment, null,
      lbLabel(props, id, labelId),
      lbShell({
        id: undefined, boxRef: boxRef, cls: cx('cds--dropdown', invalid && 'cds--dropdown--invalid', open && 'cds--dropdown--open', disabled && 'cds--dropdown--disabled', readOnly && 'cds--dropdown--readonly', inline && 'cds--dropdown--inline', sz && 'cds--dropdown--' + sz),
        size: sz, type: props.type, open: open, up: props.direction === 'top', invalid: invalid, invalidText: props.invalidText, invalidId: invalidId,
        warn: warn, warnText: props.warnText, warnId: warnId, disabled: disabled
      }, kids),
      lbHelper(props, helperId)));
}
exports.Dropdown = Dropdown;

/* ===================================================================== */
/* ComboBox                                                              */
/* ===================================================================== */
function ComboBox(props) {
  var autoId = useId(); var id = props.id || ('cbx-' + autoId);
  var labelId = id + '-label', menuId = id + '-menu', helperId = id + '-help', invalidId = id + '-err', warnId = id + '-warn';
  var items = props.items || [];
  var toStr = props.itemToString || lbText;
  var st = useState(props.initialSelectedItem !== undefined ? props.initialSelectedItem : null);
  var ctl = props.selectedItem !== undefined;
  var selected = ctl ? props.selectedItem : st[0];
  var selText = selected != null ? toStr(selected) : '';
  var iv = useState(props.inputValue !== undefined ? props.inputValue : (props.initialInputValue !== undefined ? props.initialInputValue : selText));
  var inputValue = iv[0], setInputValue = iv[1];
  var os = lbOpenState(props), open = os[0], setOpen = os[1];
  var hs = useState(-1), hi = hs[0], setHi = hs[1];
  var fs = useState(false), focused = fs[0], setFocused = fs[1];
  var boxRef = useRef(null), menuRef = useRef(null), inputRef = useRef(null);
  var disabled = props.disabled, readOnly = props.readOnly;
  var nv = lbNorm(props), invalid = nv.invalid, warn = nv.warn;

  var first = useRef(true);
  useEffect(function () { if (first.current) { first.current = false; return; } if (ctl) setInputValue(selText); }, [selText]);

  function filterBy(v) {
    if (v === selText && selected != null) return items;
    if (!v) return items;
    return items.filter(function (item) {
      if (props.shouldFilterItem) return props.shouldFilterItem({ item: item, itemToString: toStr, inputValue: v });
      return toStr(item).toLowerCase().indexOf(v.toLowerCase()) > -1;
    });
  }
  var shown = filterBy(inputValue);

  function reset() { setOpen(false); setHi(-1); if (!props.allowCustomValue) setInputValue(selText); }
  lbOutside(boxRef, open, reset);
  useEffect(function () { if (open) lbScroll(menuRef.current, hi); }, [hi, open]);
  useEffect(function () { if (open && hi < 0) lbScroll(menuRef.current, lbIndexOf(shown, selected)); }, [open]);

  function choose(item) {
    if (lbDisabled(item)) return;
    if (!ctl) st[1](item);
    setInputValue(toStr(item));
    if (props.onChange) props.onChange({ selectedItem: item });
    setOpen(false); setHi(-1);
  }
  function clear(e) {
    e.stopPropagation();
    if (disabled || readOnly) return;
    setInputValue(''); if (!ctl) st[1](null);
    if (props.onInputChange) props.onInputChange('');
    if (props.onChange) props.onChange({ selectedItem: null });
    if (inputRef.current) inputRef.current.focus();
  }
  function onInput(e) {
    var v = e.target.value; setInputValue(v);
    if (props.onInputChange) props.onInputChange(v);
    setOpen(true);
    var f = filterBy(v); setHi(lbNext(f, -1, 1));
    if (v === '' && selected != null) { if (!ctl) st[1](null); if (props.onChange) props.onChange({ selectedItem: null }); }
  }
  function onKeyDown(e) {
    if (disabled || readOnly) return;
    var k = e.key;
    if (k === 'ArrowDown' || k === 'ArrowUp') {
      e.preventDefault();
      var dir = k === 'ArrowDown' ? 1 : -1;
      if (!open) { setOpen(true); setHi(lbIndexOf(shown, selected) >= 0 ? lbIndexOf(shown, selected) : lbNext(shown, -1, dir)); }
      else setHi(lbNext(shown, hi, dir));
    } else if (k === 'Enter') {
      if (open && hi >= 0) { e.preventDefault(); choose(shown[hi]); }
      else if (props.allowCustomValue && inputValue) { if (props.onChange) props.onChange({ selectedItem: null, inputValue: inputValue }); setOpen(false); }
      else if (!open) setOpen(true);
    } else if (k === 'Escape') {
      if (open) { e.preventDefault(); e.stopPropagation(); reset(); }
      else if (inputValue) { setInputValue(''); if (!ctl) st[1](null); if (props.onChange) props.onChange({ selectedItem: null }); }
    } else if (k === 'Tab') { if (open) reset(); }
  }
  var describedBy = invalid ? invalidId : warn ? warnId : (props.helperText ? helperId : undefined);
  var sz = props.size && props.size !== 'md' ? props.size : undefined;
  var kids = h('div', { className: 'cds--list-box__field' },
    h('input', {
      ref: inputRef, id: id, type: 'text', role: 'combobox', className: cx('cds--text-input', !inputValue && 'cds--text-input--empty', focused && 'cds--combo-box--input--focus'), autoComplete: 'off',
      'aria-autocomplete': 'list', 'aria-haspopup': 'listbox', 'aria-expanded': open, 'aria-controls': menuId,
      'aria-activedescendant': open && hi >= 0 ? id + '-item-' + hi : undefined, 'aria-labelledby': props.titleText ? labelId : undefined,
      'aria-label': props.titleText ? undefined : (props['aria-label'] || props.placeholder), 'aria-describedby': describedBy,
      placeholder: props.placeholder, value: inputValue, disabled: disabled, readOnly: readOnly, title: inputValue || undefined,
      onChange: onInput, onKeyDown: onKeyDown, onFocus: function () { setFocused(true); }, onBlur: function () { setFocused(false); }
    }),
    lbInvalidIcons(invalid, warn),
    inputValue ? h('div', { role: 'button', tabIndex: -1, className: 'cds--list-box__selection', 'aria-label': 'Clear selected item', title: 'Clear selected item', onClick: clear }, h(Icon, { name: 'close', size: 16 })) : null,
    h('button', {
      type: 'button', tabIndex: -1, disabled: disabled, className: cx('cds--list-box__menu-icon', open && 'cds--list-box__menu-icon--open'), 'aria-label': open ? 'Close menu' : 'Open menu',
      onClick: function (e) { e.stopPropagation(); if (disabled || readOnly) return; if (open) reset(); else { setOpen(true); if (inputRef.current) inputRef.current.focus(); } }
    }, h(Icon, { name: 'chevron--down', size: 16 })));
  var Render = props.itemToElement;
  var menu = h('ul', { ref: menuRef, id: menuId, className: 'cds--list-box__menu', role: 'listbox', 'aria-labelledby': props.titleText ? labelId : undefined },
    open ? shown.map(function (item, i) {
      var dis = lbDisabled(item), isSel = lbSame(item, selected);
      return h('li', {
        key: i, id: id + '-item-' + i, role: 'option', 'aria-selected': isSel, 'aria-disabled': dis || undefined, disabled: dis || undefined,
        className: cx('cds--list-box__menu-item', isSel && 'cds--list-box__menu-item--active', hi === i && 'cds--list-box__menu-item--highlighted'),
        onMouseDown: function (e) { e.preventDefault(); }, onClick: function () { choose(item); }
      }, h('div', { className: 'cds--list-box__menu-item__option', title: toStr(item) },
        Render && typeof item === 'object' ? h(Render, item) : toStr(item), isSel ? lbSelectedIcon() : null));
    }) : null);
  return lbWrapper(props, 'cds--combo-box__wrapper',
    h(Fragment, null,
      lbLabel(props, id, labelId),
      lbShell({
        id: undefined, boxRef: boxRef, cls: cx('cds--combo-box', readOnly && 'cds--combo-box--readonly'), size: sz, open: open, up: props.direction === 'top', invalid: invalid, invalidText: props.invalidText, invalidId: invalidId,
        warn: warn, warnText: props.warnText, warnId: warnId, disabled: disabled
      }, [h(Fragment, { key: 'f' }, kids), h(Fragment, { key: 'm' }, menu)]),
      lbHelper(props, helperId)));
}
exports.ComboBox = ComboBox;

/* ===================================================================== */
/* MultiSelect                                                           */
/* ===================================================================== */
function MultiSelect(props) {
  var autoId = useId(); var id = props.id || ('ms-' + autoId);
  var labelId = id + '-label', menuId = id + '-menu', helperId = id + '-help', invalidId = id + '-err', warnId = id + '-warn';
  var items = props.items || [];
  var toStr = props.itemToString || lbText;
  var st = useState(props.initialSelectedItems || []);
  var ctl = props.selectedItems !== undefined;
  var selected = ctl ? props.selectedItems : st[0];
  var os = lbOpenState(props), open = os[0], setOpen = os[1];
  var hs = useState(-1), hi = hs[0], setHi = hs[1];
  var iv = useState(''), inputValue = iv[0], setInputValue = iv[1];
  var boxRef = useRef(null), menuRef = useRef(null), inputRef = useRef(null), kbd = useRef(0);
  var disabled = props.disabled, readOnly = props.readOnly, filterable = !!props.filterable;
  var nv = lbNorm(props), invalid = nv.invalid, warn = nv.warn;

  // selectionFeedback (as Carbon): 'top-after-reopen' (default) moves the items selected when the menu opened to the top;
  // 'top' keeps every selected item at the top while choosing; 'fixed' keeps the given order.
  var feedback = props.selectionFeedback || 'top-after-reopen';
  var openSel = useRef(null);
  if (!open) openSel.current = null; else if (!openSel.current) openSel.current = selected.slice();
  function orderBy(pin) {
    if (feedback === 'fixed') return items;
    var top = [], rest = [];
    items.forEach(function (it) { (lbIndexOf(pin, it) > -1 ? top : rest).push(it); });
    return top.concat(rest);
  }
  var ordered = orderBy(feedback === 'top' ? selected : (openSel.current || selected));
  function filterList(v, list) {
    return filterable && v ? (list || ordered).filter(function (it) {
      if (props.shouldFilterItem) return props.shouldFilterItem({ item: it, itemToString: toStr, inputValue: v });
      return toStr(it).toLowerCase().indexOf(v.toLowerCase()) > -1;
    }) : (list || ordered);
  }
  var shown = filterList(inputValue);

  lbOutside(boxRef, open, function () { setOpen(false); setHi(-1); setInputValue(''); });
  useEffect(function () { if (open) lbScroll(menuRef.current, hi); }, [hi, open]);

  function commit(next) {
    if (!ctl) st[1](next);
    if (props.onChange) props.onChange({ selectedItems: next });
  }
  function toggle(item) {
    if (lbDisabled(item) || disabled || readOnly) return;
    var idx = lbIndexOf(selected, item);
    var next = idx > -1 ? selected.filter(function (x, i) { return i !== idx; }) : selected.concat([item]);
    commit(next);
    // With 'top' the list reorders as items are chosen: keep the highlight on the item just toggled.
    if (feedback === 'top' && open) setHi(lbIndexOf(filterList(inputValue, orderBy(next)), item));
  }
  function clearAll(e) {
    e.stopPropagation();
    if (disabled || readOnly) return;
    commit([]);
    if (filterable && inputRef.current) inputRef.current.focus();
  }
  function onKeyDown(e) {
    if (disabled || readOnly) return;
    var k = e.key;
    if (k === 'ArrowDown' || k === 'ArrowUp') {
      e.preventDefault();
      var dir = k === 'ArrowDown' ? 1 : -1;
      if (!open) { setOpen(true); setHi(lbNext(shown, -1, dir)); } else setHi(lbNext(shown, hi, dir));
    } else if (k === 'Home' && open && !filterable) { e.preventDefault(); setHi(lbEdge(shown, false)); }
    else if (k === 'End' && open && !filterable) { e.preventDefault(); setHi(lbEdge(shown, true)); }
    else if (k === 'Enter' || (k === ' ' && !filterable)) {
      e.preventDefault();
      if (open && hi >= 0) toggle(shown[hi]); else if (!open) setOpen(true); else setOpen(false);
    } else if (k === 'Escape') {
      if (open) { e.preventDefault(); e.stopPropagation(); setOpen(false); setHi(-1); }
      else if (filterable && inputValue) setInputValue('');
    } else if (k === 'Tab') { if (open) { setOpen(false); setHi(-1); } }
    // As Carbon: Delete on the closed field clears every selection.
    else if (k === 'Delete' && !open && !filterable && selected.length) { e.preventDefault(); commit([]); }
  }
  function onToggleClick() {
    if (disabled || readOnly) return;
    if (open) { setOpen(false); setHi(-1); } else setOpen(true);
  }
  var count = selected.length;
  var tag = count > 0 ? h('div', { className: cx('cds--tag', 'cds--tag--filter', 'cds--tag--high-contrast', disabled && 'cds--tag--disabled') },
    h('span', { className: 'cds--tag__label', title: String(count) }, count),
    h('div', { role: 'button', tabIndex: -1, className: 'cds--tag__close-icon', 'aria-label': 'Clear all selected items', title: 'Clear all selected items', 'aria-disabled': readOnly ? true : undefined, onClick: clearAll }, h(Icon, { name: 'close', size: 16 }))) : null;
  var describedBy = invalid ? invalidId : warn ? warnId : (props.type !== 'inline' && props.helperText ? helperId : undefined);
  var fieldLabelId = id + '-field-label';
  var field;
  if (filterable) {
    field = h('div', { className: 'cds--list-box__field' },
      tag,
      h('input', {
        ref: inputRef, id: id, type: 'text', role: 'combobox', className: cx('cds--text-input', !inputValue && !count && 'cds--text-input--empty'), autoComplete: 'off',
        'aria-autocomplete': 'list', 'aria-haspopup': 'listbox', 'aria-expanded': open, 'aria-controls': menuId,
        'aria-activedescendant': open && hi >= 0 ? id + '-item-' + hi : undefined, 'aria-labelledby': props.titleText ? labelId : undefined,
        'aria-label': props.titleText ? undefined : (props['aria-label'] || props.placeholder || props.label), 'aria-describedby': describedBy,
        placeholder: props.placeholder || props.label, value: inputValue, disabled: disabled, readOnly: readOnly,
        onChange: function (e) {
          var v = e.target.value; setInputValue(v); setOpen(true);
          setHi(lbNext(filterList(v), -1, 1));
        },
        onKeyDown: onKeyDown
      }),
      lbInvalidIcons(invalid, warn),
      inputValue ? h('div', { role: 'button', tabIndex: -1, className: 'cds--list-box__selection', 'aria-label': 'Clear input', title: 'Clear input', onClick: function (e) { e.stopPropagation(); setInputValue(''); if (inputRef.current) inputRef.current.focus(); } }, h(Icon, { name: 'close', size: 16 })) : null,
      h('button', {
        type: 'button', tabIndex: -1, disabled: disabled, className: cx('cds--list-box__menu-icon', open && 'cds--list-box__menu-icon--open'), 'aria-label': open ? 'Close menu' : 'Open menu',
        onClick: function (e) { e.stopPropagation(); onToggleClick(); if (!open && inputRef.current) inputRef.current.focus(); }
      }, h(Icon, { name: 'chevron--down', size: 16 })));
  } else {
    field = h(Fragment, null,
      lbInvalidIcons(invalid, warn),
      h('div', { className: 'cds--list-box__field--wrapper' },
        tag,
        h('button', {
          id: id, type: 'button', className: 'cds--list-box__field', disabled: disabled, role: 'combobox', 'aria-haspopup': 'listbox', 'aria-expanded': open, 'aria-controls': menuId,
          'aria-disabled': disabled || readOnly || undefined, 'aria-labelledby': props.titleText ? labelId + ' ' + fieldLabelId : fieldLabelId, 'aria-describedby': describedBy,
          'aria-activedescendant': open && hi >= 0 ? id + '-item-' + hi : undefined, onClick: onToggleClick, onKeyDown: onKeyDown, onKeyUp: noSpace
        },
          h('span', { id: fieldLabelId, className: 'cds--list-box__label' }, props.label),
          lbMenuIcon(open))));
  }
  var menu = h('ul', { ref: menuRef, id: menuId, className: 'cds--list-box__menu', role: 'listbox', 'aria-multiselectable': true, 'aria-labelledby': props.titleText ? labelId : undefined },
    open ? shown.map(function (item, i) {
      var dis = lbDisabled(item) || disabled, on = lbIndexOf(selected, item) > -1;
      var text = toStr(item);
      return h('li', {
        key: i, id: id + '-item-' + i, role: 'option', 'aria-selected': on, 'aria-disabled': dis || undefined, disabled: dis || undefined,
        className: cx('cds--list-box__menu-item', on && 'cds--list-box__menu-item--active', hi === i && 'cds--list-box__menu-item--highlighted'),
        onMouseDown: function (e) { e.preventDefault(); },
        onClick: function () { toggle(item); }
      }, h('div', { className: 'cds--list-box__menu-item__option', title: text },
        h('div', { className: 'cds--checkbox-wrapper' },
          h('input', { type: 'checkbox', className: 'cds--checkbox', checked: on, readOnly: true, tabIndex: -1, 'aria-hidden': 'true', disabled: dis, onChange: function () {} }),
          h('label', { className: 'cds--checkbox-label' }, h('span', { className: 'cds--checkbox-label-text' }, text)))));
    }) : null);
  var sz = props.size && props.size !== 'md' ? props.size : undefined;
  var inline = props.type === 'inline';
  return lbWrapper(props, 'cds--multi-select__wrapper',
    h(Fragment, null,
      lbLabel(props, id, labelId),
      lbShell({
        id: undefined, boxRef: boxRef,
        cls: cx('cds--multi-select', filterable && 'cds--multi-select--filterable cds--combo-box', count > 0 && 'cds--multi-select--selected', inline && 'cds--multi-select--inline', readOnly && 'cds--multi-select--readonly'),
        size: sz, type: props.type, open: open, up: props.direction === 'top', invalid: invalid, invalidText: props.invalidText, invalidId: invalidId,
        warn: warn, warnText: props.warnText, warnId: warnId, disabled: disabled
      }, [h(Fragment, { key: 'f' }, field), h(Fragment, { key: 'm' }, menu)]),
      lbHelper(props, helperId)));
}
exports.MultiSelect = MultiSelect;

/* ===================================================================== */
/* FileUploader                                                          */
/* ===================================================================== */
function fuMatches(file, accept) {
  if (!accept || !accept.length) return true;
  var name = (file.name || '').toLowerCase(), type = (file.type || '').toLowerCase();
  for (var i = 0; i < accept.length; i++) {
    var a = String(accept[i]).toLowerCase();
    if (a.charAt(0) === '.') { if (name.slice(-a.length) === a) return true; }
    else if (a.slice(-2) === '/*') { if (type.indexOf(a.slice(0, -1)) === 0) return true; }
    else if (a === type) return true;
  }
  return false;
}
var fuUid = 0;
function fuSpinner(desc) {
  return h('svg', { className: 'cds--file-loading', viewBox: '0 0 100 100', role: 'img', 'aria-label': desc, focusable: 'false' },
    h('title', null, desc),
    h('circle', { className: 'cds--file-loading__background', cx: 50, cy: 50, r: 44 }),
    h('circle', { className: 'cds--file-loading__stroke', cx: 50, cy: 50, r: 44 }));
}

function FileUploaderButton(props) {
  var autoId = useId(); var inputId = props.id || ('fub-' + autoId);
  var ref = useRef(null);
  var st = useState(props.labelText); var label = st[0];
  useEffect(function () { st[1](props.labelText); }, [props.labelText]);
  var size = props.size || 'md';
  return h(Fragment, null,
    h('button', {
      type: 'button', disabled: props.disabled, className: cx('cds--btn', 'cds--btn--' + (props.buttonKind || 'primary'), 'cds--btn--' + size, 'cds--file-btn', props.className),
      onClick: function () { if (ref.current) ref.current.click(); }
    }, label),
    h('label', { className: 'cds--visually-hidden', htmlFor: inputId }, h('span', null, label)),
    h('input', {
      ref: ref, id: inputId, type: 'file', className: 'cds--visually-hidden', tabIndex: -1, disabled: props.disabled, multiple: props.multiple, name: props.name,
      accept: props.accept ? props.accept.join(',') : undefined,
      onChange: function (e) {
        var files = Array.prototype.slice.call(e.target.files || []).filter(function (f) { return fuMatches(f, props.accept); });
        // As Carbon: the button shows the chosen file name (or "n files") unless disableLabelChanges.
        if (!props.disableLabelChanges && files.length) st[1](files.length === 1 ? files[0].name : files.length + ' files');
        if (props.onChange) props.onChange(e);
        if (props.onAddFiles) props.onAddFiles(e, { addedFiles: files });
        e.target.value = '';
      }
    }));
}
exports.FileUploaderButton = FileUploaderButton;

function FileUploaderDropContainer(props) {
  var autoId = useId(); var inputId = props.id || ('fud-' + autoId);
  var ref = useRef(null);
  var as = useState(false), active = as[0], setActive = as[1];
  function add(evt, list) {
    var files = Array.prototype.slice.call(list || []).filter(function (f) { return fuMatches(f, props.accept); });
    if (!props.multiple && files.length > 1) files = files.slice(0, 1);
    if (files.length && props.onAddFiles) props.onAddFiles(evt, { addedFiles: files });
  }
  var drag = function (on) { return function (e) { e.stopPropagation(); e.preventDefault(); if (props.disabled) return; setActive(on); }; };
  return h('div', {
    className: 'cds--file', onDragOver: drag(true), onDragLeave: drag(false),
    onDrop: function (e) { e.stopPropagation(); e.preventDefault(); if (props.disabled) return; setActive(false); add(e, e.dataTransfer && e.dataTransfer.files); }
  },
    h('button', {
      type: 'button', disabled: props.disabled, className: cx('cds--file__drop-container', 'cds--file-browse-btn', active && 'cds--file__drop-container--drag-over', props.disabled && 'cds--file-browse-btn--disabled', props.className),
      onClick: function () { if (ref.current) ref.current.click(); }
    }, props.labelText || 'Drag and drop files here or click to upload'),
    h('label', { htmlFor: inputId, className: 'cds--visually-hidden' }, props.labelText || 'Drag and drop files here or click to upload'),
    h('input', {
      ref: ref, id: inputId, type: 'file', className: 'cds--file-input', tabIndex: -1, disabled: props.disabled, multiple: props.multiple, name: props.name,
      accept: props.accept ? props.accept.join(',') : undefined,
      onChange: function (e) { add(e, e.target.files); e.target.value = ''; }
    }));
}
exports.FileUploaderDropContainer = FileUploaderDropContainer;

function FileUploaderItem(props) {
  var status = props.status || 'uploading', invalid = props.invalid, name = props.name || '';
  var desc = props.iconDescription || (status === 'complete' ? 'Complete' : status === 'edit' ? 'Remove file' : 'Uploading file');
  var errId = (name.replace(/\s+/g, '') || 'file') + '-id-error';
  var state;
  if (status === 'uploading') state = fuSpinner(desc);
  else if (status === 'edit') {
    state = h(Fragment, null,
      invalid ? h(Icon, { name: 'warning--filled', size: 16, className: 'cds--file-invalid' }) : null,
      h('button', {
        type: 'button', className: 'cds--file-close', 'aria-label': desc + ' - ' + name, 'aria-describedby': invalid && props.errorSubject ? errId : undefined,
        onClick: function (e) { if (props.onDelete) props.onDelete(e, { uuid: props.uuid }); }
      }, h(Icon, { name: 'close', size: 16 })));
  } else state = h(Icon, { name: 'checkmark--filled', size: 16, title: desc, className: 'cds--file-complete' });
  return h('span', { className: cx('cds--file__selected-file', invalid && 'cds--file__selected-file--invalid', props.size === 'md' && 'cds--file__selected-file--md', props.size === 'sm' && 'cds--file__selected-file--sm', props.className) },
    h('p', { className: 'cds--file-filename', title: name, id: name.replace(/\s+/g, '') }, name),
    h('div', { className: 'cds--file-container-item' }, h('span', { className: 'cds--file__state-container' }, state)),
    invalid && props.errorSubject ? h('div', { className: 'cds--form-requirement', role: 'alert', id: errId },
      h('div', { className: 'cds--form-requirement__title' }, props.errorSubject),
      props.errorBody ? h('p', { className: 'cds--form-requirement__supplement' }, props.errorBody) : null) : null);
}
exports.FileUploaderItem = FileUploaderItem;

function FileUploader(props) {
  var variant = props.variant || 'button';
  var st = useState(props.defaultFiles || []);
  var ctl = props.files !== undefined;
  var files = ctl ? props.files : st[0];
  var disabled = props.disabled;
  function add(evt, info) {
    var added = info.addedFiles.map(function (f) { return { uuid: 'fu-' + (++fuUid), name: f.name, status: props.filenameStatus || 'edit', file: f }; });
    var next = props.multiple === false || (!props.multiple && variant === 'button') ? added.slice(0, 1) : files.concat(added);
    if (!ctl) st[1](next);
    if (props.onChange) props.onChange({ target: { files: info.addedFiles }, addedFiles: info.addedFiles, files: next });
  }
  function del(item) {
    return function (evt) {
      var next = files.filter(function (f) { return f !== item; });
      if (!ctl) st[1](next);
      if (props.onDelete) props.onDelete(evt, { uuid: item.uuid, file: item });
    };
  }
  return h('div', { className: cx('cds--form-item', props.className) },
    h('div', { className: 'cds--file-wrapper' },
      props.labelTitle ? h('strong', { className: cx('cds--file--label', disabled && 'cds--file--label--disabled') }, props.labelTitle) : null,
      props.labelDescription ? h('p', { className: cx('cds--label-description', disabled && 'cds--label-description--disabled') }, props.labelDescription) : null,
      variant === 'drop'
        ? h('div', { className: 'cds--file-container--drop' }, h(FileUploaderDropContainer, { labelText: props.dropLabel, accept: props.accept, multiple: props.multiple !== false, disabled: disabled, name: props.name, onAddFiles: add }))
        : h(FileUploaderButton, { disableLabelChanges: true, labelText: props.buttonLabel || 'Add file', buttonKind: props.buttonKind, size: props.size === 'sm' || props.size === 'lg' ? props.size : 'md', accept: props.accept, multiple: props.multiple, disabled: disabled, name: props.name, onAddFiles: add }),
      h('div', { className: 'cds--file-container' },
        files.map(function (f, i) {
          return h(FileUploaderItem, { key: f.uuid || f.name + i, uuid: f.uuid, name: f.name, status: f.status || 'edit', invalid: f.invalid, errorSubject: f.errorSubject, errorBody: f.errorBody, iconDescription: props.iconDescription, size: props.size === 'sm' || props.size === 'md' ? props.size : undefined, onDelete: del(f) });
        }))));
}
exports.FileUploader = FileUploader;

/* ===================================================================== */
/* DatePicker (single, simple and range)                                 */
/* ===================================================================== */
var DP_MONTHS = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
var DP_WD = ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'];
var DP_WD_LONG = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
function dpStrip(d) { return new Date(d.getFullYear(), d.getMonth(), d.getDate()); }
function dpSame(a, b) { return !!(a && b && a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate()); }
function dpPad(n, l) { var s = String(n); while (s.length < l) s = '0' + s; return s; }
function dpFormat(d, fmt) {
  if (!d) return '';
  return fmt.replace(/[mdjnYy]/g, function (t) {
    switch (t) {
      case 'm': return dpPad(d.getMonth() + 1, 2);
      case 'n': return String(d.getMonth() + 1);
      case 'd': return dpPad(d.getDate(), 2);
      case 'j': return String(d.getDate());
      case 'Y': return String(d.getFullYear());
      default: return dpPad(d.getFullYear() % 100, 2);
    }
  });
}
function dpParse(str, fmt) {
  if (str == null) return null;
  var order = [];
  var src = '^\\s*' + fmt.replace(/[mdjnYy]|[^mdjnYy]+/g, function (t) {
    if (/^[mdjnYy]$/.test(t)) { order.push(t); return '(\\d{1,4})'; }
    return t.replace(/[.*+?^()|[\]\\\/{}]/g, function (c) { return '\\' + c; });
  }) + '\\s*' + String.fromCharCode(36);
  var m = new RegExp(src).exec(String(str));
  if (!m) return null;
  var y = null, mo = null, d = null;
  for (var i = 0; i < order.length; i++) {
    var v = parseInt(m[i + 1], 10), t = order[i];
    if (t === 'm' || t === 'n') mo = v; else if (t === 'd' || t === 'j') d = v; else if (t === 'Y') y = v; else y = 2000 + v;
  }
  if (y == null || mo == null || d == null || mo < 1 || mo > 12 || d < 1 || y < 1000) return null;
  var date = new Date(y, mo - 1, d);
  return date.getMonth() === mo - 1 && date.getDate() === d ? date : null;
}
function dpToDate(v, fmt) {
  if (v == null || v === '') return null;
  if (v instanceof Date) return isNaN(v.getTime()) ? null : dpStrip(v);
  if (typeof v === 'number') return dpStrip(new Date(v));
  var p = dpParse(v, fmt); if (p) return p;
  var iso = /^(\d{4})-(\d{2})-(\d{2})/.exec(String(v));
  if (iso) return new Date(+iso[1], +iso[2] - 1, +iso[3]);
  var g = new Date(v); return isNaN(g.getTime()) ? null : dpStrip(g);
}
function dpToList(v, fmt) {
  if (v === undefined || v === null) return [];
  var arr = Object.prototype.toString.call(v) === '[object Array]' ? v : [v];
  var out = [];
  for (var i = 0; i < arr.length; i++) { var d = dpToDate(arr[i], fmt); if (d) out.push(d); }
  return out;
}
function dpTitle(d) { return DP_WD_LONG[d.getDay()] + ', ' + DP_MONTHS[d.getMonth()] + ' ' + d.getDate() + ', ' + d.getFullYear(); }

function DatePickerInput(props) {
  var autoId = useId(); var id = props.id || ('dpi-' + autoId);
  var type = props.datePickerType || 'single';
  var disabled = props.disabled, off = !!disabled || !!props.readOnly;
  var invalid = !!props.invalid && !off, warn = !invalid && !!props.warn && !off;
  // Inside a range DatePicker the shared message is rendered once under both fields (msgId).
  var own = !props.msgId;
  var describedBy = props.msgId && (invalid || warn) ? props.msgId : invalid ? id + '-err' : warn ? id + '-warn' : (props.helperText ? id + '-help' : undefined);
  var cls = cx('cds--date-picker__input', props.size && props.size !== 'md' && 'cds--date-picker__input--' + props.size, props.className);
  var icon = null;
  if (invalid) icon = h(Icon, { name: 'warning--filled', size: 16, className: 'cds--date-picker__icon cds--date-picker__icon--invalid' });
  else if (warn) icon = a2WarnIcon('cds--date-picker__icon cds--date-picker__icon--warn');
  else if (type !== 'simple') icon = h(Icon, { name: 'calendar', size: 16, className: 'cds--date-picker__icon' });
  return h('div', { className: cx('cds--date-picker-container', props.containerClassName) },
    props.labelText ? h('label', { htmlFor: id, className: cx('cds--label', disabled && 'cds--label--disabled', props.hideLabel && 'cds--visually-hidden') }, props.labelText) : null,
    h('div', { className: cx('cds--date-picker-input__wrapper', invalid && 'cds--date-picker-input__wrapper--invalid', warn && 'cds--date-picker-input__wrapper--warn') },
      h('span', null,
        h('input', {
          ref: props.inputRef, id: id, type: 'text', className: cls, placeholder: props.placeholder, pattern: props.pattern, value: props.value, defaultValue: props.defaultValue,
          disabled: disabled, readOnly: props.readOnly, 'data-invalid': invalid || undefined, autoComplete: 'off',
          'aria-invalid': invalid || undefined, 'aria-describedby': describedBy,
          onChange: props.onChange, onClick: props.onClick, onFocus: props.onFocus, onBlur: props.onBlur, onKeyDown: props.onKeyDown
        }),
        icon),
      props.children),
    invalid && own ? h('div', { id: id + '-err', className: 'cds--form-requirement' }, props.invalidText) : null,
    warn && own ? h('div', { id: id + '-warn', className: 'cds--form-requirement cds--form-requirement--warn' }, props.warnText) : null,
    props.helperText && !invalid && !warn ? h('div', { id: id + '-help', className: cx('cds--form__helper-text', disabled && 'cds--form__helper-text--disabled') }, props.helperText) : null);
}
exports.DatePickerInput = DatePickerInput;

function DatePicker(props) {
  var type = props.datePickerType || 'single';
  var fmt = props.dateFormat || 'm/d/Y';
  var range = type === 'range', simple = type === 'simple';
  var minD = dpToDate(props.minDate, fmt), maxD = dpToDate(props.maxDate, fmt);
  var ctl = props.value !== undefined;
  var ds = useState(dpToList(props.defaultValue, fmt));
  var dates = ctl ? dpToList(props.value, fmt) : ds[0];
  var dkey = dates.map(function (d) { return d.getTime(); }).join(',');
  function texts(list) { return [dpFormat(list[0], fmt), dpFormat(list[1], fmt)]; }
  var ts = useState(texts(dates)), text = ts[0], setText = ts[1];
  var firstSync = useRef(true);
  useEffect(function () { if (firstSync.current) { firstSync.current = false; return; } setText(texts(dates)); }, [dkey, fmt]);

  var today = dpStrip(new Date());
  var os = lbOpenState({ open: props.open, defaultOpen: props.defaultOpen, onOpenChange: props.onOpenChange, disabled: props.disabled || simple });
  var open = os[0], setOpen = os[1];
  var anchor = dates[0] || today;
  var vs = useState({ y: anchor.getFullYear(), m: anchor.getMonth() }), view = vs[0], setView = vs[1];
  var fs = useState(anchor), fd = fs[0], setFd = fs[1];
  var keyNav = useRef(false), calRef = useRef(null), rootRef = useRef(null), inputRef = useRef(null);
  var hs = useState(null), hover = hs[0], setHover = hs[1];

  lbOutside(rootRef, open, function () { setOpen(false); });
  useEffect(function () {
    if (!open || !keyNav.current || !calRef.current) return;
    var el = calRef.current.querySelector('[data-ts="' + fd.getTime() + '"]');
    if (el) { el.focus(); keyNav.current = false; }
  }, [fd, open, view.y, view.m]);

  function commit(list) {
    if (!ctl) ds[1](list);
    setText(texts(list));
    if (props.onChange) props.onChange(list);
  }
  function isOff(d) { return !!((minD && d < minD) || (maxD && d > maxD)); }
  function pick(d) {
    if (isOff(d)) return;
    setFd(d);
    if (!range) { commit([d]); setOpen(false); return; }
    if (dates.length !== 1) { commit([d]); return; }
    var s = dates[0];
    if (d < s) commit([d, s]); else commit([s, d]);
    setOpen(false);
  }
  function go(y, m, keepDay) {
    var dt = new Date(y, m, 1); y = dt.getFullYear(); m = dt.getMonth();
    setView({ y: y, m: m });
    if (keepDay) { var last = new Date(y, m + 1, 0).getDate(); setFd(new Date(y, m, Math.min(fd.getDate(), last))); }
  }
  function moveFocus(d) {
    keyNav.current = true; setFd(d);
    if (d.getFullYear() !== view.y || d.getMonth() !== view.m) setView({ y: d.getFullYear(), m: d.getMonth() });
  }
  function onDayKey(e) {
    var k = e.key, d = fd, n = null;
    if (k === 'ArrowRight') n = new Date(d.getFullYear(), d.getMonth(), d.getDate() + 1);
    else if (k === 'ArrowLeft') n = new Date(d.getFullYear(), d.getMonth(), d.getDate() - 1);
    else if (k === 'ArrowDown') n = new Date(d.getFullYear(), d.getMonth(), d.getDate() + 7);
    else if (k === 'ArrowUp') n = new Date(d.getFullYear(), d.getMonth(), d.getDate() - 7);
    else if (k === 'PageDown' || k === 'PageUp') { var mm = d.getMonth() + (k === 'PageDown' ? 1 : -1); var last = new Date(d.getFullYear(), mm + 1, 0).getDate(); n = new Date(d.getFullYear(), mm, Math.min(d.getDate(), last)); }
    else if (k === 'Home') n = new Date(d.getFullYear(), d.getMonth(), 1);
    else if (k === 'End') n = new Date(d.getFullYear(), d.getMonth() + 1, 0);
    else if (k === 'Escape') { e.preventDefault(); e.stopPropagation(); setOpen(false); if (inputRef.current) inputRef.current.focus(); return; }
    if (n) { e.preventDefault(); moveFocus(n); }
  }
  function openCal() {
    if (props.disabled || props.readOnly || simple) return;
    var a = dates[0] || today;
    if (!open) { setView({ y: a.getFullYear(), m: a.getMonth() }); setFd(a); }
    setOpen(true);
  }
  function inputKey(idx) {
    return function (e) {
      if (e.key === 'ArrowDown' && !simple && !props.disabled) { e.preventDefault(); keyNav.current = true; if (open) { var el = calRef.current && calRef.current.querySelector('[data-ts="' + fd.getTime() + '"]'); if (el) el.focus(); } else openCal(); }
      else if (e.key === 'Escape' && open) { e.stopPropagation(); setOpen(false); }
      else if (e.key === 'Enter') applyText(idx);
    };
  }
  function applyText(idx) {
    var s = text[idx];
    if (!s) { var l = dates.filter(function (d, i) { return i !== idx; }); if (dates.length > idx) commit(l); return; }
    var d = dpParse(s, fmt);
    if (!d || isOff(d)) { setText(texts(dates)); return; }
    var next = range ? (idx === 0 ? [d].concat(dates.slice(1)) : [dates[0] || d, d]) : [d];
    if (range && next.length === 2 && next[1] < next[0]) next = [next[1], next[0]];
    setView({ y: d.getFullYear(), m: d.getMonth() }); setFd(d);
    if (next.length === dates.length && next.every(function (x, i) { return dpSame(x, dates[i]); })) { setText(texts(dates)); return; }
    commit(next);
  }
  function changeText(idx) {
    return function (e) {
      var v = e.target.value, nt = text.slice(); nt[idx] = v; setText(nt);
      var d = dpParse(v, fmt);
      if (d && !isOff(d)) { setView({ y: d.getFullYear(), m: d.getMonth() }); setFd(d); }
    };
  }
  function blurText(idx) { return function () { applyText(idx); }; }

  // calendar ---------------------------------------------------------------
  var first = new Date(view.y, view.m, 1), lead = first.getDay();
  var cells = [];
  for (var i = 0; i < 42; i++) cells.push(new Date(view.y, view.m, 1 - lead + i));
  var s0 = dates[0], s1 = dates[1];
  var preview = range && dates.length === 1 && hover && !isOff(hover) ? hover : null;
  var rs = s0, re = s1 || preview; if (rs && re && re < rs) { var tmp = rs; rs = re; re = tmp; }
  var fdVisible = cells.some(function (c) { return dpSame(c, fd); });
  var prevOff = !!(minD && new Date(view.y, view.m, 0) < minD), nextOff = !!(maxD && new Date(view.y, view.m + 1, 1) > maxD);
  var calendar = open ? h('div', { ref: calRef, className: 'flatpickr-calendar open animate', role: 'dialog', 'aria-label': 'Choose date', tabIndex: -1 },
    h('div', { className: 'flatpickr-months' },
      h('button', { type: 'button', className: cx('flatpickr-prev-month', prevOff && 'disabled'), 'aria-label': 'Previous month', disabled: prevOff, onClick: function () { go(view.y, view.m - 1, true); } }, h(Icon, { name: 'chevron--left', size: 16 })),
      h('div', { className: 'flatpickr-month' },
        h('div', { className: 'flatpickr-current-month' },
          h('span', { className: 'cur-month', 'aria-live': 'polite' }, DP_MONTHS[view.m]),
          h('div', { className: 'numInputWrapper' },
            h('input', {
              className: 'numInput cur-year', type: 'number', 'aria-label': 'Year', value: view.y, min: 1000, max: 9999,
              onChange: function (e) { var y = parseInt(e.target.value, 10); if (y > 0) setView({ y: y, m: view.m }); }
            }),
            h('span', { className: 'arrowUp', 'aria-hidden': 'true', onClick: function () { go(view.y + 1, view.m, true); } }),
            h('span', { className: 'arrowDown', 'aria-hidden': 'true', onClick: function () { go(view.y - 1, view.m, true); } })))),
      h('button', { type: 'button', className: cx('flatpickr-next-month', nextOff && 'disabled'), 'aria-label': 'Next month', disabled: nextOff, onClick: function () { go(view.y, view.m + 1, true); } }, h(Icon, { name: 'chevron--right', size: 16 }))),
    h('div', { className: 'flatpickr-innerContainer' },
      h('div', { className: 'flatpickr-rContainer' },
        h('div', { className: 'flatpickr-weekdays' }, h('div', { className: 'flatpickr-weekdaycontainer' }, DP_WD.map(function (w, i) { return h('span', { key: i, className: 'flatpickr-weekday', title: DP_WD_LONG[i] }, w); }))),
        h('div', { className: 'flatpickr-days' },
          h('div', { className: 'dayContainer', role: 'group', 'aria-label': DP_MONTHS[view.m] + ' ' + view.y, onKeyDown: onDayKey, onMouseLeave: function () { setHover(null); } },
            cells.map(function (d, i) {
              var out = d.getMonth() !== view.m, off = isOff(d);
              var isSel = dpSame(d, s0) || dpSame(d, s1);
              var inR = rs && re && d > rs && d < re;
              var tab = fdVisible ? dpSame(d, fd) : (!out && d.getDate() === 1);
              return h('button', {
                key: d.getTime(), type: 'button', 'data-ts': d.getTime(), tabIndex: tab ? 0 : -1, disabled: off, 'aria-label': dpTitle(d), 'aria-pressed': isSel,
                className: cx('flatpickr-day', out && (d < first ? 'prevMonthDay' : 'nextMonthDay'), dpSame(d, today) && 'today', isSel && 'selected', inR && 'inRange', range && rs && re && dpSame(d, rs) && 'startRange', range && rs && re && dpSame(d, re) && 'endRange', off && 'flatpickr-disabled'),
                onClick: function () { pick(d); }, onMouseEnter: function () { if (range && dates.length === 1) setHover(d); }
              }, d.getDate());
            }))))))
  : null;

  var autoId = useId(), rangeMsgId = 'dp-msg-' + autoId;
  var off = !!props.disabled || !!props.readOnly;
  var rInvalid = range && !!props.invalid && !off, rWarn = range && !rInvalid && !!props.warn && !off;
  var common = { msgId: range ? rangeMsgId : undefined, size: props.size, disabled: props.disabled, readOnly: props.readOnly, invalid: props.invalid, invalidText: props.invalidText, warn: props.warn, warnText: props.warnText, datePickerType: type, pattern: props.pattern, hideLabel: props.hideLabel };
  var ph = props.placeholder || fmt.replace(/[mdjnYy]/g, function (t) { return { m: 'mm', n: 'mm', d: 'dd', j: 'dd', Y: 'yyyy', y: 'yy' }[t]; });
  var f0 = Object.assign({}, common, {
    id: props.id, labelText: props.labelText, placeholder: ph, helperText: props.helperText, value: text[0], onChange: changeText(0), onBlur: blurText(0), onClick: openCal, onFocus: undefined,
    onKeyDown: inputKey(0), inputRef: inputRef, children: calendar
  });
  var f1 = range ? Object.assign({}, common, { id: props.id ? props.id + '-end' : undefined, labelText: props.endLabelText || 'End date', placeholder: props.endPlaceholder || ph, value: text[1], onChange: changeText(1), onBlur: blurText(1), onClick: openCal, onKeyDown: inputKey(1) }) : null;
  return h('div', { className: cx('cds--form-item', props.className) },
    h('div', { ref: rootRef, className: cx('cds--date-picker', 'cds--date-picker--' + type, props.short && 'cds--date-picker--short', props.light && 'cds--date-picker--light') },
      h(DatePickerInput, f0), f1 ? h(DatePickerInput, f1) : null),
    rInvalid ? h('div', { id: rangeMsgId, className: 'cds--form-requirement' }, props.invalidText) : null,
    rWarn ? h('div', { id: rangeMsgId, className: 'cds--form-requirement cds--form-requirement--warn' }, props.warnText) : null);
}
exports.DatePicker = DatePicker;

/* ===================================================================== */
/* Skeletons                                                             */
/* ===================================================================== */
function DropdownSkeleton(props) {
  var size = props.size || 'md';
  return h('div', { className: cx('cds--form-item', props.className) },
    props.hideLabel ? null : h('span', { className: 'cds--label cds--skeleton' }),
    h('div', { className: cx('cds--skeleton', 'cds--dropdown-v2', 'cds--list-box', 'cds--list-box--' + size) },
      h('div', { className: 'cds--list-box__field' }, h('span', { className: 'cds--list-box__label' }))));
}
exports.DropdownSkeleton = DropdownSkeleton;

function DatePickerSkeleton(props) {
  var range = !!props.range;
  function field(k) {
    return h('div', { key: k, className: 'cds--date-picker-container' },
      props.hideLabel ? null : h('span', { className: 'cds--label cds--skeleton' }),
      h('div', { className: 'cds--date-picker__input cds--skeleton' }));
  }
  return h('div', { className: cx('cds--form-item', props.className) },
    h('div', { className: cx('cds--date-picker', 'cds--skeleton', range ? 'cds--date-picker--range' : 'cds--date-picker--single') },
      field('a'), range ? field('b') : null));
}
exports.DatePickerSkeleton = DatePickerSkeleton;

function FileUploaderSkeleton(props) {
  return h('div', { className: cx('cds--form-item', 'cds--file-uploader--skeleton', props.className) },
    h('span', { className: 'cds--skeleton__text cds--skeleton__heading', style: { width: '100px' } }),
    h('span', { className: 'cds--skeleton__text cds--label-description', style: { width: '225px' } }),
    h('span', { className: 'cds--skeleton__placeholder cds--file-btn--skeleton' }));
}
exports.FileUploaderSkeleton = FileUploaderSkeleton;

})(Carbon,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx);

/* ---- B1 ---- */
(function(exports,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx){
/* @components: DataTable, Table, TableHead, TableRow, TableHeader, TableBody, TableCell, Pagination, PaginationNav, StructuredList, StructuredListHead, StructuredListBody, StructuredListRow, StructuredListCell, ContainedList, ContainedListItem */

// ---------------------------------------------------------------- shared helpers
function useControlled(ctrl, def) {
  var st = useState(def);
  var controlled = ctrl !== undefined;
  return [controlled ? ctrl : st[0], function (v) { if (!controlled) st[1](v); }];
}
function inlineGlyph(body, size, className) {
  return h('svg', { className: cx('cds--icon', className), width: size, height: size, viewBox: '0 0 32 32', focusable: 'false', 'aria-hidden': 'true', dangerouslySetInnerHTML: htmlOf(body) });
}
var ARROWS_VERTICAL = '<polygon points="27.6,20.6 24,24.2 24,4 22,4 22,24.2 18.4,20.6 17,22 23,28 29,22"/><polygon points="9,4 3,10 4.4,11.4 8,7.8 8,28 10,28 10,7.8 13.6,11.4 15,10"/>';
var RADIO_OFF = '<path d="M16,2A14,14,0,1,0,30,16,14,14,0,0,0,16,2Zm0,26A12,12,0,1,1,28,16,12,12,0,0,1,16,28Z"/>';
var RADIO_ON = RADIO_OFF + '<path d="M16,10a6,6,0,1,0,6,6A6,6,0,0,0,16,10Z"/>';

// ---------------------------------------------------------------- Table primitives
function Table(props) {
  var size = props.size || 'lg';
  var rest = Object.assign({}, props);
  ['size', 'useZebraStyles', 'isSortable', 'stickyHeader', 'overflowMenuOnHover', 'className', 'children', 'contentClassName'].forEach(function (k) { delete rest[k]; });
  return h('div', { className: cx('cds--data-table-content', props.stickyHeader && 'cds--data-table-content--sticky-header', props.contentClassName) },
    h('table', Object.assign({}, rest, {
      className: cx('cds--data-table', 'cds--data-table--' + size, props.useZebraStyles && 'cds--data-table--zebra', props.isSortable && 'cds--data-table--sort', props.stickyHeader && 'cds--data-table--sticky-header', props.overflowMenuOnHover === false && 'cds--data-table--visible-overflow-menu', props.className)
    }), props.children));
}
function TableHead(props) { return h('thead', props); }
function TableBody(props) { return h('tbody', props); }
function TableRow(props) {
  var rest = Object.assign({}, props);
  delete rest.isSelected; delete rest.className;
  return h('tr', Object.assign({}, rest, { className: cx(props.isSelected && 'cds--data-table--selected', props.className) }));
}
function TableCell(props) { return h('td', props); }
function TableHeader(props) {
  var sortable = props.isSortable, dir = props.sortDirection || 'NONE';
  var align = props.align;
  if (!sortable) {
    return h('th', { id: props.id, scope: props.scope || 'col', colSpan: props.colSpan, align: align, className: props.className },
      props.children ? h('div', { className: 'cds--table-header-label' }, props.children) : null);
  }
  var active = props.isSortHeader && dir !== 'NONE';
  var ariaSort = !props.isSortHeader || dir === 'NONE' ? 'none' : (dir === 'DESC' ? 'descending' : 'ascending');
  return h('th', { id: props.id, scope: props.scope || 'col', colSpan: props.colSpan, align: align, 'aria-sort': ariaSort, className: cx(props.className, 'cds--table-sort__header') },
    h('button', { type: 'button', onClick: props.onClick, className: cx('cds--table-sort', active && 'cds--table-sort--active', props.isSortHeader && dir === 'DESC' && 'cds--table-sort--descending') },
      h('span', { className: 'cds--table-sort__flex' },
        h('div', { className: 'cds--table-header-label' }, props.children),
        h(Icon, { name: 'arrow--up', size: 20, className: 'cds--table-sort__icon' }),
        inlineGlyph(ARROWS_VERTICAL, 20, 'cds--table-sort__icon-unsorted'))));
}
exports.Table = Table; exports.TableHead = TableHead; exports.TableBody = TableBody; exports.TableRow = TableRow; exports.TableCell = TableCell; exports.TableHeader = TableHeader;

// ---------------------------------------------------------------- DataTable internals
function InlineCheckbox(p) {
  var ref = useRef(null);
  useEffect(function () { if (ref.current) ref.current.indeterminate = !!p.indeterminate; }, [p.indeterminate, p.checked]);
  return h('div', { className: 'cds--checkbox--inline' },
    h('input', { ref: ref, type: 'checkbox', id: p.id, className: 'cds--checkbox', checked: !!p.checked, disabled: p.disabled, onChange: p.onChange, 'aria-label': p.label }),
    h('label', { htmlFor: p.id, className: 'cds--checkbox-label', 'aria-label': p.label, title: p.label }));
}
function InlineRadio(p) {
  return h('div', { className: 'cds--radio-button-wrapper' },
    h('input', { type: 'radio', id: p.id, name: p.name, className: 'cds--radio-button', checked: !!p.checked, disabled: p.disabled, onChange: p.onChange }),
    h('label', { htmlFor: p.id, className: 'cds--radio-button__label' },
      h('span', { className: 'cds--radio-button__appearance' }),
      h('span', { className: 'cds--radio-button__label-text cds--visually-hidden' }, p.label)));
}
function ToolbarSearch(p) {
  var ref = useRef(null);
  var st = useState(!!p.persistent || !!p.defaultExpanded); var expanded = st[0], setExpanded = st[1];
  var value = p.value || '';
  var active = p.persistent || expanded || !!value;
  var closeVisible = !!value && active;
  function clear() { p.onChange(''); if (ref.current) ref.current.focus(); }
  return h('div', {
    role: 'search', 'aria-label': p.placeholder,
    className: cx('cds--search', 'cds--search--' + p.size, p.persistent ? 'cds--toolbar-search-container-persistent' : 'cds--toolbar-search-container-expandable', active && 'cds--toolbar-search-container-active')
  },
    h(Icon, { name: 'search', size: 16, className: 'cds--search-magnifier-icon' }),
    h('label', { htmlFor: p.id, className: 'cds--label' }, p.placeholder),
    h('input', {
      ref: ref, id: p.id, type: 'text', className: 'cds--search-input', placeholder: p.placeholder, value: value, autoComplete: 'off',
      onChange: function (e) { p.onChange(e.target.value); },
      onFocus: function () { if (!p.persistent) setExpanded(true); },
      onBlur: function () { if (!p.persistent && !value) setExpanded(false); },
      onKeyDown: function (e) { if (e.key === 'Escape' && value) { e.stopPropagation(); p.onChange(''); } }
    }),
    h('button', { type: 'button', className: cx('cds--search-close', !closeVisible && 'cds--search-close--hidden'), 'aria-label': 'Clear search input', title: 'Clear search input', disabled: !closeVisible, onClick: clear },
      h(Icon, { name: 'close', size: 16 })));
}
function sortCompare(a, b) {
  if (typeof a === 'number' && typeof b === 'number') return a - b;
  return String(a == null ? '' : a).localeCompare(String(b == null ? '' : b), undefined, { numeric: true, sensitivity: 'base' });
}
function cellText(v) { return (typeof v === 'string' || typeof v === 'number') ? String(v) : ''; }

function DataTable(props) {
  var autoId = useId(); var tid = props.id || ('dt-' + autoId);
  var headers = props.headers || [], rows = props.rows || [];
  var size = props.size || 'lg';
  var sel = props.selectable === true ? 'multi' : (props.selectable === 'radio' || props.selectable === 'single' || props.radio) ? 'radio' : (props.selectable === 'multi' ? 'multi' : false);
  var expandable = !!props.expandable;
  var sortableTable = props.isSortable !== undefined ? props.isSortable : props.sortable;

  var sortState = useControlled(props.sortKey === undefined ? undefined : { key: props.sortKey, dir: props.sortDirection || 'NONE' }, { key: props.defaultSortKey || null, dir: props.defaultSortDirection || 'NONE' });
  var sort = sortState[0];
  var selState = useControlled(props.selectedIds, props.defaultSelectedIds || []);
  var selected = selState[0];
  var expState = useControlled(props.expandedIds, props.defaultExpandedIds || []);
  var expandedIds = expState[0];
  var qState = useControlled(props.searchValue, props.defaultSearchValue || '');
  var query = qState[0];
  var hoverState = useState(null); var hoverId = hoverState[0], setHoverId = hoverState[1];
  var menuState = useState(null); var openMenuId = menuState[0], setOpenMenuId = menuState[1];
  var hasRowActions = !!props.rowActions;
  var menuSize = size === 'xl' ? 'lg' : size;

  function setSelected(ids) { selState[1](ids); if (props.onSelectionChange) props.onSelectionChange(ids); }
  function toggleSort(key) {
    var next = sort.key !== key ? 'ASC' : (sort.dir === 'NONE' ? 'ASC' : sort.dir === 'ASC' ? 'DESC' : 'NONE');
    var v = { key: key, dir: next };
    sortState[1](v);
    if (props.onSortChange) props.onSortChange(key, next);
  }
  function setQuery(v) { qState[1](v); if (props.onSearchChange) props.onSearchChange(v); }

  // filter + sort
  var visible = rows;
  var q = (query || '').trim().toLowerCase();
  if (q) {
    visible = rows.filter(function (r) {
      return headers.some(function (hd) { return cellText(r[hd.key]).toLowerCase().indexOf(q) !== -1; });
    });
  }
  if (sort.key && sort.dir !== 'NONE') {
    var k = sort.key, m = sort.dir === 'DESC' ? -1 : 1;
    visible = visible.map(function (r, i) { return { r: r, i: i }; }).sort(function (a, b) {
      var c = sortCompare(a.r[k], b.r[k]) * m; return c || (a.i - b.i);
    }).map(function (o) { return o.r; });
  }

  var selectableRows = visible.filter(function (r) { return !r.disabled; });
  var selectedVisible = selectableRows.filter(function (r) { return selected.indexOf(r.id) !== -1; });
  var allSelected = selectableRows.length > 0 && selectedVisible.length === selectableRows.length;
  var someSelected = selectedVisible.length > 0 && !allSelected;
  var selectedCount = rows.filter(function (r) { return selected.indexOf(r.id) !== -1; }).length;

  function toggleAll() {
    var ids = selectableRows.map(function (r) { return r.id; });
    if (allSelected) setSelected(selected.filter(function (id) { return ids.indexOf(id) === -1; }));
    else setSelected(selected.concat(ids.filter(function (id) { return selected.indexOf(id) === -1; })));
  }
  function toggleRow(row) {
    if (sel === 'radio') { setSelected([row.id]); return; }
    var on = selected.indexOf(row.id) !== -1;
    setSelected(on ? selected.filter(function (id) { return id !== row.id; }) : selected.concat([row.id]));
  }
  function toggleExpand(row) {
    var on = expandedIds.indexOf(row.id) !== -1;
    var next = on ? expandedIds.filter(function (id) { return id !== row.id; }) : expandedIds.concat([row.id]);
    expState[1](next);
    if (props.onExpandChange) props.onExpandChange(next);
  }

  var batchActions = sel === 'multi' ? (props.batchActions || []) : [];
  var showBatch = batchActions.length > 0 && selectedCount > 0;
  var hasToolbar = !!props.search || batchActions.length > 0 || !!props.toolbarContent;
  var smToolbar = size === 'xs' || size === 'sm';
  var colCount = headers.length + (expandable ? 1 : 0) + (sel ? 1 : 0) + (hasRowActions ? 1 : 0);

  var cancelBatch = function () { setSelected([]); };

  // header row
  var headCells = [];
  if (expandable) headCells.push(h('th', { key: '_x', scope: 'col', className: 'cds--table-expand' }));
  if (sel === 'multi') headCells.push(h('th', { key: '_s', scope: 'col', className: 'cds--table-column-checkbox' },
    h(InlineCheckbox, { id: tid + '-all', label: 'Select all rows', checked: allSelected, indeterminate: someSelected, disabled: selectableRows.length === 0, onChange: toggleAll })));
  if (sel === 'radio') headCells.push(h('th', { key: '_s', scope: 'col', className: 'cds--table-column-checkbox cds--table-column-radio' }));
  headers.forEach(function (hd) {
    var sortable = hd.isSortable !== undefined ? hd.isSortable : sortableTable;
    headCells.push(h(TableHeader, { key: hd.key, id: tid + '-h-' + hd.key, align: hd.align, isSortable: !!sortable, isSortHeader: sort.key === hd.key, sortDirection: sort.key === hd.key ? sort.dir : 'NONE', onClick: function () { toggleSort(hd.key); } }, hd.header));
  });
  if (hasRowActions) headCells.push(h('th', { key: '_m', scope: 'col', className: 'cds--table-column-menu' }, h('span', { className: 'cds--visually-hidden' }, props.rowActionsHeader || 'Actions')));

  // overflow-menu column (Carbon composes an OverflowMenu inside a TableCell; here it is built from rowActions)
  function rowMenu(row) {
    var OM = exports.OverflowMenu, MI = exports.MenuItem;
    var acts = typeof props.rowActions === 'function' ? props.rowActions(row) : props.rowActions;
    if (!OM || !MI || !acts || !acts.length) return null;
    var name = headers.length ? cellText(row[headers[0].key]) : '';
    return h(OM, {
      size: menuSize, flipped: true, iconDescription: name ? 'Options for ' + name : 'Options',
      onOpen: function () { setOpenMenuId(row.id); },
      onClose: function () { setOpenMenuId(function (cur) { return cur === row.id ? null : cur; }); }
    }, acts.map(function (a, i) {
      return h(MI, { key: i, label: a.label, kind: a.kind, disabled: a.disabled, renderIcon: a.icon, onClick: function () { if (a.onClick) a.onClick(row); } });
    }));
  }

  // body rows
  var bodyRows = [];
  var groupIndex = 0;
  visible.forEach(function (row) {
    var isSel = selected.indexOf(row.id) !== -1;
    var isExp = expandable && expandedIds.indexOf(row.id) !== -1;
    var zebra = expandable ? (groupIndex % 2 === 1 ? 'even' : 'odd') : undefined;
    groupIndex++;
    var cells = [];
    if (expandable) cells.push(h('td', { key: '_x', className: 'cds--table-expand', 'data-previous-value': isExp ? 'collapsed' : undefined },
      h('button', { type: 'button', className: 'cds--table-expand__button', 'aria-label': isExp ? 'Collapse current row' : 'Expand current row', 'aria-expanded': isExp, 'aria-controls': tid + '-c-' + row.id, onClick: function () { toggleExpand(row); } },
        h(Icon, { name: 'chevron--right', size: 16, className: 'cds--table-expand__svg' }))));
    if (sel) cells.push(h('td', { key: '_s', className: cx('cds--table-column-checkbox', sel === 'radio' && 'cds--table-column-radio') },
      sel === 'radio'
        ? h(InlineRadio, { id: tid + '-s-' + row.id, name: tid + '-sel', label: 'Select row', checked: isSel, disabled: row.disabled, onChange: function () { toggleRow(row); } })
        : h(InlineCheckbox, { id: tid + '-s-' + row.id, label: 'Select row', checked: isSel, disabled: row.disabled, onChange: function () { toggleRow(row); } })));
    headers.forEach(function (hd) {
      var v = row[hd.key];
      cells.push(h('td', { key: hd.key, align: hd.align }, props.renderCell ? props.renderCell(v, row, hd) : v));
    });
    if (hasRowActions) cells.push(h('td', { key: '_m', className: 'cds--table-column-menu' }, rowMenu(row)));
    bodyRows.push(h('tr', {
      key: row.id,
      'data-zebra': zebra,
      className: cx(expandable && 'cds--parent-row', isExp && 'cds--expandable-row', isSel && 'cds--data-table--selected', hoverId === row.id && isExp && 'cds--expandable-row--hover'),
      'data-parent-row': expandable ? true : undefined
    }, cells));
    if (isExp) {
      bodyRows.push(h('tr', {
        key: row.id + '-c', id: tid + '-c-' + row.id, 'data-child-row': true, 'data-zebra': zebra, className: 'cds--expandable-row',
        onMouseEnter: function () { setHoverId(row.id); }, onMouseLeave: function () { setHoverId(null); }
      }, h('td', { colSpan: colCount }, h('div', { className: 'cds--child-row-inner-container' }, props.renderExpanded ? props.renderExpanded(row) : null))));
    }
  });

  var empty = props.emptyState;
  var emptyNode = null;
  if (empty && visible.length === 0) {
    var isObj = empty && typeof empty === 'object' && !React.isValidElement(empty);
    emptyNode = h('div', { className: 'cds--data-table-empty' }, isObj ? [
      empty.title ? h('p', { key: 't', className: 'cds--data-table-empty__title' }, empty.title) : null,
      empty.description ? h('p', { key: 'd', className: 'cds--data-table-empty__description' }, empty.description) : null,
      empty.action ? h('div', { key: 'a', className: 'cds--data-table-empty__action' }, empty.action) : null
    ] : empty);
  }

  var Btn = exports.Button;
  var titleId = tid + '-title', descId = tid + '-description';
  var labelledBy = props['aria-label'] ? undefined : (props.title ? titleId : props['aria-labelledby']);
  return h('div', { className: cx('cds--data-table-container', props.stickyHeader && 'cds--data-table-container--sticky-header', props.className), style: props.style },
    (props.title || props.description) ? h('div', { className: 'cds--data-table-header' },
      props.title ? h('h4', { id: titleId, className: 'cds--data-table-header__title' }, props.title) : null,
      props.description ? h('p', { id: descId, className: 'cds--data-table-header__description' }, props.description) : null) : null,
    hasToolbar ? h('section', { className: cx('cds--table-toolbar', smToolbar && 'cds--table-toolbar--sm'), 'aria-label': props.toolbarLabel || 'data table toolbar' },
      batchActions.length ? h('div', { className: cx('cds--batch-actions', showBatch && 'cds--batch-actions--active'), 'aria-hidden': !showBatch },
        h('div', { className: 'cds--batch-summary' },
          h('p', { className: 'cds--batch-summary__para' }, h('span', null, selectedCount + (selectedCount === 1 ? ' item selected' : ' items selected')))),
        h('div', { className: 'cds--action-list' },
          batchActions.map(function (a, i) {
            return h(Btn, { key: i, kind: 'primary', renderIcon: a.icon, tabIndex: showBatch ? 0 : -1, onClick: function () { if (a.onClick) a.onClick(rows.filter(function (r) { return selected.indexOf(r.id) !== -1; })); } }, a.label);
          }),
          h(Btn, { kind: 'primary', className: 'cds--batch-summary__cancel', tabIndex: showBatch ? 0 : -1, onClick: cancelBatch }, 'Cancel'))) : null,
      h('div', { className: 'cds--toolbar-content' },
        props.search ? h(ToolbarSearch, { id: tid + '-search', size: smToolbar ? 'sm' : 'lg', persistent: !!props.persistentSearch, placeholder: props.searchPlaceholder || 'Search', value: query, onChange: setQuery }) : null,
        props.toolbarContent)) : null,
    h(Table, { size: size, useZebraStyles: props.useZebraStyles, isSortable: !!sortableTable, stickyHeader: !!props.stickyHeader, overflowMenuOnHover: props.overflowMenuOnHover, contentClassName: openMenuId !== null && !props.stickyHeader ? 'cds--data-table-content--menu-open' : undefined, 'aria-labelledby': labelledBy, 'aria-describedby': props.description ? descId : undefined, 'aria-label': props['aria-label'] },
      h(TableHead, null, h('tr', null, headCells)),
      h(TableBody, null, bodyRows)),
    emptyNode);
}
exports.DataTable = DataTable;

// ---------------------------------------------------------------- Pagination
function PagSelect(p) {
  return h('div', { className: 'cds--form-item' },
    h('div', { className: cx('cds--select cds--select--inline', p.className) },
      p.labelText ? h('label', { htmlFor: p.id, className: 'cds--label cds--visually-hidden' }, p.labelText) : null,
      h('div', { className: 'cds--select-input--inline__wrapper' },
        h('div', { className: 'cds--select-input__wrapper' },
          h('select', { id: p.id, className: 'cds--select-input', value: p.value, disabled: p.disabled, onChange: p.onChange }, p.children),
          h(Icon, { name: 'chevron--down', size: 16, className: 'cds--select__arrow' })))));
}
function paginationSizes(sizes) {
  sizes = sizes && sizes.length ? sizes : [10];
  return typeof sizes[0] === 'object' ? sizes : sizes.map(function (s) { return { text: s, value: s }; });
}
function Pagination(props) {
  var autoId = useId(); var id = props.id !== undefined ? String(props.id) : autoId.replace(/:/g, '');
  var sizes = paginationSizes(props.pageSizes);
  function pick(ps) { var f = sizes.filter(function (s) { return s.value === ps; })[0]; return f ? f.value : sizes[0].value; }
  var size = props.size || 'md';
  var pstate = useState(props.page || 1); var page = pstate[0], setPage = pstate[1];
  var sstate = useState(pick(props.pageSize)); var pageSize = sstate[0], setPageSize = sstate[1];
  useEffect(function () { if (props.page !== undefined) setPage(props.page); }, [props.page]);
  useEffect(function () { if (props.pageSize !== undefined) setPageSize(pick(props.pageSize)); }, [props.pageSize]);
  var backRef = useRef(null), fwdRef = useRef(null);
  var focusState = useState(null); var focusTarget = focusState[0], setFocusTarget = focusState[1];
  useEffect(function () {
    if (focusTarget) { var r = focusTarget === 'backward' ? backRef : fwdRef; if (r.current && !r.current.disabled) r.current.focus(); setFocusTarget(null); }
  }, [focusTarget]);

  var totalItems = props.totalItems;
  var totalPages = totalItems ? Math.max(Math.ceil(totalItems / pageSize), 1) : 1;
  var disabled = !!props.disabled;
  var pagesUnknown = !!props.pagesUnknown;
  var backDisabled = disabled || page === 1;
  var fwdDisabled = disabled || (page === totalPages && !pagesUnknown) || !!props.isLastPage;
  var itemText = props.itemText || function (min, max) { return min + '–' + max + ' items'; };
  var itemRangeText = props.itemRangeText || function (min, max, total) { return min + '–' + max + ' of ' + total + ' items'; };
  var pageRangeText = props.pageRangeText || function (cur, total) { return 'of ' + total + (total === 1 ? ' page' : ' pages'); };
  var pageText = props.pageText || function (p) { return 'page ' + p; };

  function emit(ch) { if (props.onChange) props.onChange(ch); }
  function onSize(e) { var ps = Number(e.target.value); setPage(1); setPageSize(ps); emit({ page: 1, pageSize: ps }); }
  function onPage(e) { var p = Number(e.target.value); if (p > 0 && p <= totalPages) { setPage(p); emit({ page: p, pageSize: pageSize }); } }
  function inc() { var n = page + 1; setPage(n); if (n === totalPages) setFocusTarget('backward'); emit({ page: n, pageSize: pageSize }); }
  function dec() { var n = page - 1; setPage(n); if (n === 1) setFocusTarget('forward'); emit({ page: n, pageSize: pageSize }); }

  var pageOpts = []; for (var i = 1; i <= totalPages; i++) pageOpts.push(h('option', { key: i, value: i, className: 'cds--select-option' }, String(i)));
  var rangeLabel = (pagesUnknown || !totalItems)
    ? (totalItems === 0 ? itemRangeText(0, 0, 0) : itemText(pageSize * (page - 1) + 1, page * pageSize))
    : itemRangeText(Math.min(pageSize * (page - 1) + 1, totalItems), Math.min(page * pageSize, totalItems), totalItems);
  var selId = 'cds-pagination-select-' + id;

  return h('div', { className: cx('cds--pagination', 'cds--pagination--' + size, props.className) },
    h('div', { className: 'cds--pagination__left' },
      h('label', { id: selId + '-count-label', className: 'cds--pagination__text', htmlFor: selId }, props.itemsPerPageText || 'Items per page:'),
      h(PagSelect, { id: selId, className: 'cds--select__item-count', value: pageSize, disabled: props.pageSizeInputDisabled || disabled, onChange: onSize },
        sizes.map(function (s) { return h('option', { key: s.value, value: s.value, className: 'cds--select-option' }, String(s.text)); })),
      h('span', { className: 'cds--pagination__text cds--pagination__items-count' }, rangeLabel)),
    h('div', { className: 'cds--pagination__right' },
      pagesUnknown
        ? h('span', { className: 'cds--pagination__text cds--pagination__page-text cds--pagination__unknown-pages-text' }, pageText(page))
        : [
          h(PagSelect, { key: 's', id: selId + '-right', className: 'cds--select__page-number', labelText: 'Page of ' + totalPages + ' pages', value: page, disabled: props.pageInputDisabled || disabled, onChange: onPage }, pageOpts),
          h('span', { key: 't', className: 'cds--pagination__text' }, pageRangeText(page, totalPages))
        ],
      h('div', { className: 'cds--pagination__control-buttons' },
        h('button', { type: 'button', ref: backRef, className: cx('cds--pagination__button', 'cds--pagination__button--backward', backDisabled && 'cds--pagination__button--no-index'), disabled: backDisabled, 'aria-label': props.backwardText || 'Previous page', title: props.backwardText || 'Previous page', onClick: dec }, h(Icon, { name: 'caret--left', size: 16 })),
        h('button', { type: 'button', ref: fwdRef, className: cx('cds--pagination__button', 'cds--pagination__button--forward', fwdDisabled && 'cds--pagination__button--no-index'), disabled: fwdDisabled, 'aria-label': props.forwardText || 'Next page', title: props.forwardText || 'Next page', onClick: inc }, h(Icon, { name: 'caret--right', size: 16 })))));
}
exports.Pagination = Pagination;

// ---------------------------------------------------------------- PaginationNav
function calculateCuts(page, totalItems, itemsDisplayedOnPage, splitPoint) {
  if (itemsDisplayedOnPage >= totalItems) return { front: 0, back: 0 };
  var split = splitPoint || Math.ceil(itemsDisplayedOnPage / 2) - 1;
  var frontHidden = page + 1 - split;
  var backHidden = totalItems - page - (itemsDisplayedOnPage - split) + 1;
  if (frontHidden <= 1) { backHidden -= frontHidden <= 0 ? Math.abs(frontHidden) + 1 : 0; frontHidden = 0; }
  if (backHidden <= 1) { frontHidden -= backHidden <= 0 ? Math.abs(backHidden) + 1 : 0; backHidden = 0; }
  return { front: frontHidden, back: backHidden };
}
function PnItem(p) {
  return h('li', { className: 'cds--pagination-nav__list-item' },
    h('button', { type: 'button', className: cx('cds--pagination-nav__page', p.active && 'cds--pagination-nav__page--active'), onClick: p.onClick, 'data-page': p.page, 'aria-current': p.active ? 'page' : undefined },
      h('span', { className: 'cds--pagination-nav__accessibility-label' }, p.active ? 'Active, Page' : 'Page'), p.page));
}
function PnOverflow(p) {
  if (!(p.count > 1)) {
    if (p.count === 1) return h(PnItem, { page: p.fromIndex + 1, onClick: function () { p.onSelect(p.fromIndex); } });
    return null;
  }
  var opts = [h('option', { key: 'blank', value: '', hidden: true })];
  for (var i = 0; i < p.count; i++) opts.push(h('option', { key: i, value: String(p.fromIndex + i) }, String(p.fromIndex + i + 1)));
  return h('li', { className: 'cds--pagination-nav__list-item' },
    h('div', { className: 'cds--pagination-nav__select' },
      h('select', { className: 'cds--pagination-nav__page cds--pagination-nav__page--select', 'aria-label': 'Select Page number', disabled: p.disableOverflow, value: '', onChange: function (e) { p.onSelect(Number(e.target.value)); } }, opts),
      h('div', { className: 'cds--pagination-nav__select-icon-wrapper' }, h(Icon, { name: 'overflow-menu--horizontal', size: 16, className: 'cds--pagination-nav__select-icon' }))));
}
function PnDirection(p) {
  return h('li', { className: 'cds--pagination-nav__list-item' },
    h('button', { type: 'button', className: 'cds--pagination-nav__page cds--pagination-nav__page--direction', disabled: p.disabled, onClick: p.onClick, 'aria-label': p.label, title: p.label },
      h(Icon, { name: p.direction === 'forward' ? 'caret--right' : 'caret--left', size: 16, className: 'cds--pagination-nav__icon' })));
}
function PaginationNav(props) {
  var totalItems = props.totalItems === undefined ? NaN : props.totalItems;
  var size = props.size || 'lg';
  var itemsShown = props.itemsShown === undefined ? 10 : props.itemsShown;
  var loop = !!props.loop;
  var numberOfPages = size === 'md' ? (itemsShown === 4 ? itemsShown : 5) : size === 'sm' ? Math.min(Math.max(itemsShown, 4), 7) : 4;
  var itemsDisplayed = Math.max(itemsShown >= 4 ? itemsShown : numberOfPages, 4);
  var pst = useState(props.page || 0); var currentPage = pst[0], setCurrentPage = pst[1];
  var cst = useState(function () { return calculateCuts(props.page || 0, totalItems, itemsDisplayed); }); var cuts = cst[0], setCuts = cst[1];
  var prevRef = useRef(null);
  var prevPage = prevRef.current;
  useEffect(function () { prevRef.current = currentPage; });

  function jumpTo(i) { if (i >= 0 && i < totalItems) { setCurrentPage(i); if (props.onChange) props.onChange(i); } }
  function next() { var n = currentPage + 1; if (n >= totalItems) { if (loop) jumpTo(0); } else jumpTo(n); }
  function prev() { var n = currentPage - 1; if (n < 0) { if (loop) jumpTo(totalItems - 1); } else jumpTo(n); }
  function wouldBeHidden(pg) {
    var startOffset = itemsDisplayed <= 4 && pg > 1 ? 0 : 1;
    var front = (pg >= startOffset && pg <= cuts.front) || pg === 0;
    var back = pg >= totalItems - cuts.back - 1 && pg <= totalItems - 2;
    return front || back;
  }
  useEffect(function () { if (props.page !== undefined) setCurrentPage(props.page); }, [props.page]);
  useEffect(function () { setCuts(calculateCuts(currentPage, totalItems, itemsDisplayed)); }, [totalItems, itemsShown, size]);
  useEffect(function () {
    if (wouldBeHidden(currentPage)) {
      var delta = currentPage - (prevPage || 0);
      var split = delta > 0 ? itemsDisplayed - 3 : (itemsDisplayed > 4 ? 2 : 1);
      setCuts(calculateCuts(currentPage, totalItems, itemsDisplayed, split));
    }
  }, [currentPage]);

  var startOffset = itemsDisplayed <= 4 && currentPage > 1 ? 0 : 1;
  var middle = [];
  for (var i = 0; i < totalItems; i++) middle.push(i);
  middle = middle.slice(startOffset + cuts.front, (1 + cuts.back) * -1);

  return h('nav', { className: cx('cds--pagination-nav', 'cds--pagination-nav--' + size, props.className), 'aria-label': props['aria-label'] || 'pagination' },
    h('ul', { className: 'cds--pagination-nav__list' },
      h(PnDirection, { direction: 'backward', label: 'Previous', disabled: !loop && currentPage === 0, onClick: prev }),
      (itemsDisplayed >= 5 || (itemsDisplayed <= 4 && currentPage <= 1)) ? h(PnItem, { page: 1, active: currentPage === 0, onClick: function () { jumpTo(0); } }) : null,
      h(PnOverflow, { fromIndex: startOffset, count: cuts.front, onSelect: jumpTo, disableOverflow: props.disableOverflow }),
      middle.map(function (it) { return h(PnItem, { key: it, page: it + 1, active: currentPage === it, onClick: function () { jumpTo(it); } }); }),
      h(PnOverflow, { fromIndex: totalItems - cuts.back - 1, count: cuts.back, onSelect: jumpTo, disableOverflow: props.disableOverflow }),
      totalItems > 1 ? h(PnItem, { page: totalItems, active: currentPage === totalItems - 1, onClick: function () { jumpTo(totalItems - 1); } }) : null,
      h(PnDirection, { direction: 'forward', label: 'Next', disabled: !loop && currentPage === totalItems - 1, onClick: next })),
    h('div', { 'aria-live': 'polite', 'aria-atomic': 'true', className: 'cds--pagination-nav__accessibility-label' }, 'Page ' + (currentPage + 1) + ' of ' + totalItems));
}
exports.PaginationNav = PaginationNav;

// ---------------------------------------------------------------- StructuredList
var SLSelected = React.createContext({ selected: null, set: null, name: '' });
var SLRow = React.createContext(null);
function StructuredListCell(props) {
  var cls = cx(props.head ? 'cds--structured-list-th' : 'cds--structured-list-td', props.noWrap && 'cds--structured-list-content--nowrap', props.className);
  return h('div', { className: cls, role: props.head ? 'columnheader' : 'cell', style: props.style }, props.children);
}
function StructuredListHead(props) { return h('div', { role: 'rowgroup', className: cx('cds--structured-list-thead', props.className) }, props.children); }
function StructuredListBody(props) { return h('div', { role: 'rowgroup', className: cx('cds--structured-list-tbody', props.className) }, props.children); }
function StructuredListRow(props) {
  var ctx = React.useContext(SLSelected);
  var autoId = useId(); var rowId = props.id || ('sl-' + autoId);
  var fst = useState(false); var focused = fst[0], setFocused = fst[1];
  var selection = ctx.selection;
  var isSel = selection && ctx.selected === rowId;
  if (props.head) {
    return h('div', { role: 'row', className: cx('cds--structured-list-row', 'cds--structured-list-row--header-row', props.className) },
      selection ? h(StructuredListCell, { head: true }) : null, props.children);
  }
  return h('div', {
    role: 'row',
    className: cx('cds--structured-list-row', isSel && 'cds--structured-list-row--selected', focused && 'cds--structured-list-row--focused-within', selection && props.disabled && 'cds--structured-list-row--disabled', props.className),
    'aria-disabled': selection && props.disabled ? true : undefined,
    onClick: function (e) { if (selection && ctx.set && !props.disabled) ctx.set(rowId); if (props.onClick) props.onClick(e); },
    onFocus: function () { setFocused(true); },
    onBlur: function () { setFocused(false); }
  },
    selection ? h(StructuredListCell, null,
      h('input', { type: 'radio', className: 'cds--structured-list-input cds--visually-hidden', name: ctx.name, value: rowId, checked: !!isSel, disabled: props.disabled, 'aria-label': props.label || 'Select row', onChange: function () { if (ctx.set) ctx.set(rowId); } }),
      inlineGlyph(isSel ? RADIO_ON : RADIO_OFF, 16, 'cds--structured-list__icon')) : null,
    props.children);
}
function StructuredList(props) {
  var autoId = useId();
  var st = useControlled(props.selectedRow, props.selectedInitialRow === undefined ? null : props.selectedInitialRow);
  var selection = !!props.selection;
  var ctx = { selection: selection, selected: st[0], name: 'sl-' + autoId, set: function (id) { st[1](id); if (props.onSelectionChange) props.onSelectionChange(id); } };
  var children = props.children;
  if (props.columns || props.rows) {
    children = [
      props.columns ? h(StructuredListHead, { key: 'h' }, h(StructuredListRow, { head: true }, props.columns.map(function (c, i) { return h(StructuredListCell, { head: true, key: i }, c); }))) : null,
      h(StructuredListBody, { key: 'b' }, (props.rows || []).map(function (r, ri) {
        return h(StructuredListRow, { key: r.id === undefined ? ri : r.id, id: r.id === undefined ? undefined : String(r.id), disabled: r.disabled }, (r.cells || []).map(function (c, i) { return h(StructuredListCell, { key: i, noWrap: props.noWrap }, c); }));
      }))
    ];
  }
  return h(SLSelected.Provider, { value: ctx },
    h('div', {
      role: 'table', 'aria-label': props['aria-label'] || 'Structured list section',
      className: cx('cds--structured-list', selection && 'cds--structured-list--selection', props.isCondensed && 'cds--structured-list--condensed', props.isFlush && !selection && 'cds--structured-list--flush', props.className)
    }, children));
}
exports.StructuredList = StructuredList; exports.StructuredListHead = StructuredListHead; exports.StructuredListBody = StructuredListBody; exports.StructuredListRow = StructuredListRow; exports.StructuredListCell = StructuredListCell;

// ---------------------------------------------------------------- ContainedList
function ContainedList(props) {
  var autoId = useId(); var labelId = 'cl-' + autoId + '-header';
  var kind = props.kind || 'on-page';
  // A Search / ExpandableSearch passed as a child is lifted out of the <ul> and pinned under the header.
  var searches = [], items = [];
  React.Children.toArray(props.children).forEach(function (c) {
    var isSearch = React.isValidElement(c) && (c.type === exports.Search || c.type === exports.ExpandableSearch);
    (isSearch ? searches : items).push(c);
  });
  var actionIsSearch = React.isValidElement(props.action) && (props.action.type === exports.Search || props.action.type === exports.ExpandableSearch);
  var cls = cx('cds--contained-list', props.isInset && 'cds--contained-list--inset-rulers', props.size && 'cds--contained-list--' + props.size, 'cds--contained-list--' + kind, actionIsSearch && 'cds--contained-list--action-search', props.className);
  return h('div', { className: cls },
    props.label ? h('div', { className: 'cds--contained-list__header' },
      h('div', { id: labelId, className: 'cds--contained-list__label' }, props.label),
      props.action ? h('div', { className: 'cds--contained-list__action' }, props.action) : null) : null,
    searches.length ? h('div', { className: 'cds--contained-list__search' }, searches) : null,
    items.length ? h('ul', { role: 'list', 'aria-labelledby': props.label ? labelId : props['aria-labelledby'], 'aria-label': props.label ? undefined : props['aria-label'] }, items) : null);
}
function ContainedListItem(props) {
  var clickable = !!props.onClick;
  var content = [
    props.renderIcon ? h('div', { key: 'i', className: 'cds--contained-list-item__icon' }, h(Icon, { name: props.renderIcon, size: 32, style: { width: '16px', height: '16px' } })) : null,
    h('div', { key: 'c' }, props.children)
  ];
  return h('li', { className: cx('cds--contained-list-item', clickable && 'cds--contained-list-item--clickable', props.renderIcon && 'cds--contained-list-item--with-icon', props.action && 'cds--contained-list-item--with-action', props.className) },
    clickable
      ? h('button', { type: 'button', className: 'cds--contained-list-item__content', disabled: props.disabled, onClick: props.onClick }, content)
      : h('div', { className: 'cds--contained-list-item__content' }, content),
    props.action ? h('div', { className: 'cds--contained-list-item__action' }, props.action) : null);
}
exports.ContainedList = ContainedList; exports.ContainedListItem = ContainedListItem;

})(Carbon,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx);

/* ---- B2 ---- */
(function(exports,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx){
/* @components: Accordion, AccordionItem, TreeView, TreeNode, CodeSnippet, ProgressBar, ProgressIndicator, ProgressStep, SkeletonText, SkeletonPlaceholder, Loading, InlineLoading */

// Icon helper: always ask for the full (uncropped) glyph and size it with CSS.
function B2Ico(name, px, className, title, size) {
  return h(Icon, { name: name, size: size || 32, className: className, title: title, style: { width: px + 'px', height: px + 'px' } });
}
function B2Count(children) { return React.Children.toArray(children).length; }

// ---------------------------------------------------------------- Accordion
var AccordionCtx = React.createContext({ disabled: false });

function Accordion(props) {
  var align = props.align || 'end';
  var size = props.size || 'md';
  var cls = cx('cds--accordion', 'cds--accordion--' + size, 'cds--accordion--' + align,
    props.isFlush && align !== 'start' && 'cds--accordion--flush', props.className);
  return h(AccordionCtx.Provider, { value: { disabled: !!props.disabled } },
    h(props.ordered ? 'ol' : 'ul', { className: cls }, props.children));
}

function AccordionItem(props) {
  var ctx = React.useContext(AccordionCtx);
  var autoId = useId(); var id = 'accordion-item-' + autoId;
  var s = useState(!!props.open); var isOpen = s[0], setOpen = s[1];
  var p = useState(!!props.open); var prevOpen = p[0], setPrevOpen = p[1];
  if (!!props.open !== prevOpen) { setOpen(!!props.open); setPrevOpen(!!props.open); }
  var disabled = typeof props.disabled === 'boolean' ? props.disabled : ctx.disabled;
  var cls = cx('cds--accordion__item', isOpen && !disabled && 'cds--accordion__item--active', disabled && 'cds--accordion__item--disabled', props.className);
  function onClick(e) {
    var next = !isOpen; setOpen(next);
    if (props.onHeadingClick) props.onHeadingClick({ isOpen: next, event: e });
    if (props.onClick) props.onClick(e);
  }
  function onKeyDown(e) { if (isOpen && e.key === 'Escape') setOpen(false); }
  return h('li', { className: cls },
    h('button', { type: 'button', className: 'cds--accordion__heading', disabled: disabled, 'aria-controls': id, 'aria-expanded': isOpen && !disabled, 'aria-label': props['aria-label'], onClick: onClick, onKeyDown: onKeyDown },
      B2Ico('chevron--right', 16, 'cds--accordion__arrow', undefined, 16),
      h('div', { className: 'cds--accordion__title' }, props.title === undefined ? 'title' : props.title)),
    h('div', { className: 'cds--accordion__wrapper' },
      h('div', { id: id, className: 'cds--accordion__content' }, props.children)));
}

// ----------------------------------------------------------------- TreeView
var TreeCtx = React.createContext(null);
var TreeDepthCtx = React.createContext(0);

function TreeView(props) {
  var autoId = useId(); var labelId = 'tree-label-' + autoId;
  var size = props.size || 'sm';
  var ref = useRef(null);
  // As in Carbon: `active` / `selected` seed the state and re-sync when their values change; clicks still move them.
  var aS = useState(props.active); var selS = useState(props.selected || []);
  var selKey = JSON.stringify(props.selected || []);
  var syncS = useState({ a: props.active, s: selKey }); var synced = syncS[0];
  if (synced.a !== props.active || synced.s !== selKey) {
    syncS[1]({ a: props.active, s: selKey });
    if (synced.a !== props.active) aS[1](props.active);
    if (synced.s !== selKey) selS[1](props.selected || []);
  }
  var active = aS[0];
  var selected = selS[0];

  function visibleItems() {
    var ul = ref.current; if (!ul) return [];
    return Array.prototype.filter.call(ul.querySelectorAll('[role=treeitem]'), function (n) {
      return n.getAttribute('aria-disabled') !== 'true' && !n.closest('.cds--tree-node--hidden');
    });
  }
  function refreshTab(focused) {
    var items = visibleItems(); if (!items.length) return;
    var target = focused && items.indexOf(focused) >= 0 ? focused : null;
    if (!target) target = items.filter(function (n) { return n.classList.contains('cds--tree-node--active'); })[0] || items.filter(function (n) { return n.classList.contains('cds--tree-node--selected'); })[0] || items[0];
    items.forEach(function (n) { n.tabIndex = n === target ? 0 : -1; });
  }
  useEffect(function () { refreshTab(null); });

  function onTreeSelect(e, node) {
    var multi = props.multiselect && (e.ctrlKey || e.metaKey);
    var next;
    if (multi) {
      next = selected.indexOf(node.id) >= 0 ? selected.filter(function (x) { return x !== node.id; }) : selected.concat(node.id);
    } else next = [node.id];
    selS[1](next);
    if (!(multi && next.indexOf(node.id) < 0)) { aS[1](node.id); if (props.onActivate) props.onActivate(node); }
    if (props.onSelect) props.onSelect(e, Object.assign({ selected: next }, node));
  }
  function onKeyDown(e) {
    var item = e.target.closest ? e.target.closest('[role=treeitem]') : null; if (!item) return;
    var key = e.key; if (['ArrowUp', 'ArrowDown', 'Home', 'End'].indexOf(key) < 0) return;
    var items = visibleItems(); var i = items.indexOf(item); var n = i;
    if (key === 'ArrowUp') n = Math.max(0, i - 1);
    if (key === 'ArrowDown') n = Math.min(items.length - 1, i + 1);
    if (key === 'Home') n = 0;
    if (key === 'End') n = items.length - 1;
    e.preventDefault(); e.stopPropagation();
    if (items[n]) items[n].focus();
  }
  function onFocus(e) { var item = e.target.closest ? e.target.closest('[role=treeitem]') : null; if (item && e.target === item) refreshTab(item); }

  var ctx = { active: active, selected: selected, onTreeSelect: onTreeSelect, refreshTab: function () { setTimeout(function () { refreshTab(document.activeElement); }, 0); } };
  return h('div', { className: props.className },
    props.label ? h('label', { id: labelId, className: cx('cds--label', props.hideLabel && 'cds--visually-hidden') }, props.label) : null,
    h(TreeCtx.Provider, { value: ctx },
      h('ul', { ref: ref, role: 'tree', className: cx('cds--tree', 'cds--tree--' + size), 'aria-labelledby': props.label ? labelId : undefined, 'aria-label': props.label ? undefined : props['aria-label'], 'aria-multiselectable': props.multiselect || undefined, onKeyDown: onKeyDown, onFocus: onFocus },
        h(TreeDepthCtx.Provider, { value: 0 }, props.children))));
}

function TreeNode(props) {
  var tree = React.useContext(TreeCtx) || {};
  var depth = React.useContext(TreeDepthCtx);
  var autoId = useId(); var id = props.id || ('tree-node-' + autoId);
  var hasChildren = B2Count(props.children) > 0;
  var eS = useState(props.isExpanded !== undefined ? !!props.isExpanded : !!props.defaultIsExpanded);
  var expanded = eS[0], setExpanded = eS[1];
  useEffect(function () { if (props.isExpanded !== undefined) setExpanded(!!props.isExpanded); }, [props.isExpanded]);
  var ref = useRef(null);
  var disabled = props.disabled;
  var icon = props.renderIcon;
  var isActive = tree.active === id;
  var isSelected = (tree.selected || []).indexOf(id) >= 0;
  var node = { id: id, label: props.label, value: props.value };

  function toggle(e, next) {
    setExpanded(next);
    if (props.onToggle) props.onToggle(e, { id: id, isExpanded: next, label: props.label, value: props.value });
    if (tree.refreshTab) tree.refreshTab();
  }
  function select(e) {
    e.stopPropagation(); if (disabled) return;
    if (tree.onTreeSelect) tree.onTreeSelect(e, node);
    if (props.onSelect) props.onSelect(e, node);
  }
  function onKeyDown(e) {
    if (disabled) return;
    var k = e.key;
    if (k === 'ArrowLeft' || k === 'ArrowRight' || k === 'Enter') e.stopPropagation();
    if (k === 'ArrowLeft') {
      if (hasChildren && expanded) toggle(e, false);
      else { var par = ref.current && ref.current.parentElement ? ref.current.parentElement.closest('[role=treeitem]') : null; if (par) par.focus(); }
    }
    if (k === 'ArrowRight' && hasChildren) {
      if (expanded) { var first = ref.current.querySelector(':scope > ul > [role=treeitem]'); if (first) first.focus(); }
      else toggle(e, true);
    }
    if (k === 'Enter' || k === ' ') {
      e.preventDefault();
      if (k === 'Enter' && hasChildren) toggle(e, !expanded);
      select(e);
    }
  }
  // Indentation as in TreeNode.tsx: the label is pulled back to the tree edge and re-padded.
  var off = hasChildren ? (icon ? depth + 1 + depth * 0.5 : depth + 1) : (icon ? depth + 2 + depth * 0.5 : depth + 2.5);
  var cls = cx(props.className, 'cds--tree-node', isActive && 'cds--tree-node--active', disabled && 'cds--tree-node--disabled', isSelected && 'cds--tree-node--selected', icon && 'cds--tree-node--with-icon', hasChildren ? 'cds--tree-parent-node' : 'cds--tree-leaf-node');
  return h('li', { ref: ref, id: id, role: 'treeitem', className: cls, tabIndex: disabled ? undefined : -1,
      'aria-expanded': hasChildren ? expanded : undefined, 'aria-selected': disabled ? undefined : isSelected, 'aria-current': isActive || undefined, 'aria-disabled': disabled || undefined, 'aria-owns': hasChildren ? id + '-subtree' : undefined,
      onClick: select, onKeyDown: onKeyDown },
    h('div', { className: 'cds--tree-node__label', style: { marginInlineStart: '-' + off + 'rem', paddingInlineStart: off + 'rem' } },
      hasChildren ? h('span', { className: 'cds--tree-parent-node__toggle', onClick: function (e) { if (disabled) return; e.stopPropagation(); toggle(e, !expanded); } },
        h('svg', { className: cx('cds--tree-parent-node__toggle-icon', expanded && 'cds--tree-parent-node__toggle-icon--expanded'), width: 16, height: 16, viewBox: '0 0 16 16', 'aria-hidden': 'true', focusable: 'false' }, h('path', { d: 'M8 11L3 6 13 6z' }))) : null,
      h('span', { className: 'cds--tree-node__label__details' },
        icon ? B2Ico(icon, 16, 'cds--tree-node__icon') : null,
        h('span', { id: id + '__label', className: 'cds--tree-node__label__text', title: typeof props.label === 'string' ? props.label : undefined }, props.label))),
    hasChildren ? h('ul', { id: id + '-subtree', role: 'group', 'aria-labelledby': id + '__label', className: cx('cds--tree-node__children', !expanded && 'cds--tree-node--hidden') },
      h(TreeDepthCtx.Provider, { value: depth + 1 }, props.children)) : null);
}

// -------------------------------------------------------------- CodeSnippet
function B2CopyText(text) {
  function fallback() {
    var ta = document.createElement('textarea'); ta.value = text; ta.setAttribute('readonly', ''); ta.style.position = 'fixed'; ta.style.opacity = '0';
    document.body.appendChild(ta); ta.select(); try { document.execCommand('copy'); } catch (e) {} document.body.removeChild(ta);
  }
  try { if (navigator.clipboard && navigator.clipboard.writeText) { navigator.clipboard.writeText(text).catch(fallback); return; } } catch (e) {}
  fallback();
}

function CodeSnippet(props) {
  var type = props.type || 'single';
  var feedbackText = props.feedback || 'Copied!';
  var timeout = props.feedbackTimeout === undefined ? 2000 : props.feedbackTimeout;
  var ariaLabel = props['aria-label'] || props.ariaLabel || 'Copy to clipboard';
  var description = props.copyButtonDescription || 'Copy to clipboard';
  var maxC = props.maxCollapsedNumberOfRows === undefined ? 15 : props.maxCollapsedNumberOfRows;
  var maxE = props.maxExpandedNumberOfRows === undefined ? 0 : props.maxExpandedNumberOfRows;
  var minC = props.minCollapsedNumberOfRows === undefined ? 3 : props.minCollapsedNumberOfRows;
  var minE = props.minExpandedNumberOfRows === undefined ? 16 : props.minExpandedNumberOfRows;
  var ROW = 16;
  var autoId = useId(); var uid = 'snippet-' + autoId;
  var codeRef = useRef(null), timer = useRef(null), timer2 = useRef(null);
  var xs = useState(false), expanded = xs[0], setExpanded = xs[1];
  var ms = useState(false), showMore = ms[0], setShowMore = ms[1];
  var fs = useState(0), fb = fs[0], setFb = fs[1]; // 0 hidden, 1 visible, 2 fading out

  useLayoutEffect(function () {
    if (type !== 'multi' || !codeRef.current) return;
    var el = codeRef.current;
    function measure() {
      var height = el.getBoundingClientRect().height;
      setShowMore(maxC > 0 && (maxE <= 0 || maxE > maxC) && height > maxC * ROW);
      if (expanded && minE > 0 && height <= minE * ROW) setExpanded(false);
    }
    measure();
    if (typeof ResizeObserver === 'undefined') return;
    var ro = new ResizeObserver(measure); ro.observe(el);
    return function () { ro.disconnect(); };
  }, [type, expanded, maxC, maxE, minE, props.children]);
  useEffect(function () { return function () { clearTimeout(timer.current); clearTimeout(timer2.current); }; }, []);

  function copy(e) {
    if (props.disabled) return;
    var text = props.copyText !== undefined ? props.copyText : (codeRef.current ? codeRef.current.innerText || codeRef.current.textContent : '');
    B2CopyText(text);
    setFb(1); clearTimeout(timer.current); clearTimeout(timer2.current);
    timer.current = setTimeout(function () { setFb(2); timer2.current = setTimeout(function () { setFb(0); }, 110); }, timeout);
    if (props.onClick) props.onClick(e);
  }
  var fbCls = fb ? cx('cds--copy-btn--animating', fb === 1 ? 'cds--copy-btn--fade-in' : 'cds--copy-btn--fade-out') : '';
  var cls = cx(props.className, 'cds--snippet', 'cds--snippet--' + type, type !== 'inline' && props.disabled && 'cds--snippet--disabled', expanded && 'cds--snippet--expand', props.light && 'cds--snippet--light', props.hideCopyButton && 'cds--snippet--no-copy', props.wrapText && 'cds--snippet--wraptext');

  if (type === 'inline') {
    if (props.hideCopyButton) return h('span', { className: cls }, h('code', { id: uid, ref: codeRef }, props.children));
    return h('button', { type: 'button', className: cx(cls, fbCls), disabled: props.disabled, 'aria-label': ariaLabel, 'aria-describedby': uid, onClick: copy },
      h('code', { id: uid, ref: codeRef }, props.children),
      h('span', { className: 'cds--copy-btn__feedback', role: 'status' }, fb ? feedbackText : ''));
  }

  var style = {};
  if (type === 'multi') {
    if (expanded) { if (maxE > 0) style.maxHeight = maxE * ROW; if (minE > 0) style.minHeight = minE * ROW; }
    else { if (maxC > 0) style.maxHeight = maxC * ROW; if (minC > 0) style.minHeight = minC * ROW; }
  }
  var align = props.align || 'bottom-end';
  return h('div', { className: cls },
    h('div', { role: 'textbox', tabIndex: props.disabled ? undefined : 0, className: 'cds--snippet-container', 'aria-label': props['aria-label'] || props.ariaLabel || 'code-snippet', 'aria-readonly': true, 'aria-multiline': type === 'multi' ? true : undefined, style: style },
      h('pre', { style: style }, h('code', { ref: codeRef }, props.children))),
    props.hideCopyButton ? null : h('div', { className: 'cds--popover-container cds--snippet__copy', 'data-align': align },
      h('button', { type: 'button', className: cx('cds--copy-btn', 'cds--copy-btn--' + (type === 'multi' ? 'sm' : 'md')), disabled: props.disabled, 'aria-label': description, onClick: copy },
        B2Ico('copy', 16, 'cds--snippet__icon')),
      props.disabled ? null : h('span', { className: cx('cds--copy-btn__feedback', fbCls), role: 'status' }, fb ? feedbackText : description)),
    showMore ? h('button', { type: 'button', className: 'cds--snippet-btn--expand', disabled: props.disabled, onClick: function () { setExpanded(!expanded); } },
      h('span', { className: 'cds--snippet-btn--text' }, expanded ? (props.showLessText || 'Show less') : (props.showMoreText || 'Show more')),
      B2Ico('chevron--down', 16, 'cds--icon-chevron--down cds--snippet__icon', undefined, 16)) : null);
}

// -------------------------------------------------------------- ProgressBar
function ProgressBar(props) {
  var autoId = useId();
  var labelId = 'pb-' + autoId, helperId = 'pb-help-' + autoId;
  var max = props.max === undefined ? 100 : props.max;
  var size = props.size || 'big', type = props.type || 'default', status = props.status || 'active';
  var finished = status === 'finished', error = status === 'error';
  var value = props.value;
  var indeterminate = !finished && !error && (value === null || value === undefined);
  var capped = value;
  if (capped && capped > max) capped = max;
  if (capped && capped < 0) capped = 0;
  if (error) capped = 0; else if (finished) capped = max;
  var pct = (capped === undefined || capped === null ? NaN : capped) / max;
  var barStyle = (!finished && !error && !isNaN(pct)) ? { transform: 'scaleX(' + pct + ')' } : undefined;
  var cls = cx('cds--progress-bar', 'cds--progress-bar--' + size, 'cds--progress-bar--' + type, indeterminate && 'cds--progress-bar--indeterminate', finished && 'cds--progress-bar--finished', error && 'cds--progress-bar--error', props.className);
  var statusIcon = error ? B2Ico('error--filled', 16, 'cds--progress-bar__status-icon', undefined, 16) : (finished ? B2Ico('checkmark--filled', 16, 'cds--progress-bar__status-icon', undefined, 16) : null);
  return h('div', { className: cls },
    h('div', { className: cx('cds--progress-bar__label', props.hideLabel && 'cds--visually-hidden'), id: labelId },
      h('span', { className: 'cds--progress-bar__label-text' }, props.label), statusIcon),
    h('div', { className: 'cds--progress-bar__track', role: 'progressbar', 'aria-busy': !finished, 'aria-invalid': error, 'aria-labelledby': labelId, 'aria-describedby': props.helperText ? helperId : undefined,
        'aria-valuemin': !indeterminate ? 0 : undefined, 'aria-valuemax': !indeterminate ? max : undefined, 'aria-valuenow': !indeterminate ? capped : undefined },
      h('div', { className: 'cds--progress-bar__bar', style: barStyle })),
    props.helperText ? h('div', { id: helperId, className: 'cds--progress-bar__helper-text' }, props.helperText,
      h('div', { className: 'cds--visually-hidden', 'aria-live': 'polite' }, finished ? 'Done' : 'Loading')) : null);
}

// -------------------------------------------------------- ProgressIndicator
function ProgressStep(props) {
  var current = props.current, complete = props.complete, invalid = props.invalid, disabled = props.disabled, onClick = props.onClick;
  var cls = cx('cds--progress-step', current && 'cds--progress-step--current', complete && 'cds--progress-step--complete', !complete && !current && 'cds--progress-step--incomplete', disabled && 'cds--progress-step--disabled', props.className);
  var icon;
  var desc = props.description;
  if (invalid) icon = B2Ico('warning', 16, 'cds--progress__warning', desc, 16);
  else if (current) icon = B2Ico('incomplete', 16, null, desc);
  else if (complete) icon = B2Ico('checkmark--outline', 16, null, desc);
  else icon = B2Ico('circle-dash', 16, null, desc);
  var message = invalid ? 'Invalid' : complete ? 'Complete' : current ? 'Current' : 'Incomplete';
  return h('li', { className: cls },
    h('button', { type: 'button', className: cx('cds--progress-step-button', (!onClick || current) && 'cds--progress-step-button--unclickable'), disabled: disabled, 'aria-disabled': disabled, tabIndex: !current && onClick && !disabled ? 0 : -1, onClick: !current && !disabled ? onClick : undefined, title: props.label },
      icon,
      h('div', { className: 'cds--progress-text' },
        h('p', { className: 'cds--progress-label' }, props.label),
        props.secondaryLabel !== null && props.secondaryLabel !== undefined ? h('p', { className: 'cds--progress-optional' }, props.secondaryLabel) : null),
      h('span', { className: 'cds--visually-hidden' }, message),
      h('span', { className: 'cds--progress-line' })));
}

function ProgressIndicator(props) {
  var controlled = props.currentIndex === undefined ? 0 : props.currentIndex;
  var s = useState(controlled); var cur = s[0], setCur = s[1];
  var p = useState(controlled); var prev = p[0], setPrev = p[1];
  if (controlled !== prev) { setCur(controlled); setPrev(controlled); }
  var cls = cx('cds--progress', props.vertical && 'cds--progress--vertical', props.spaceEqually && !props.vertical && 'cds--progress--space-equal', props.className);
  return h('ul', { className: cls }, React.Children.map(props.children, function (child, index) {
    if (!React.isValidElement(child)) return null;
    var onClick = props.onChange ? function () { props.onChange(index); } : undefined;
    if (index === cur) return React.cloneElement(child, { complete: child.props.complete, current: child.props.complete ? false : true, index: index, onClick: onClick });
    if (index < cur) return React.cloneElement(child, { complete: true, index: index, onClick: onClick });
    return React.cloneElement(child, { complete: child.props.complete || false, index: index, onClick: onClick });
  }));
}

// ----------------------------------------------------------------- Skeletons
function SkeletonText(props) {
  var lineCount = props.lineCount || 3;
  var width = props.width || '100%';
  var base = cx('cds--skeleton__text', props.heading && 'cds--skeleton__heading', props.className);
  if (!props.paragraph) return h('p', { className: base, style: { width: width } });
  var widths = ['100%', '85%', '95%', '75%', '90%'];
  var lines = [];
  for (var i = 0; i < lineCount; i++) {
    var w = width !== '100%' ? width : widths[i % widths.length];
    lines.push(h('p', { key: i, className: base, style: { width: w } }));
  }
  return h('div', null, lines);
}

function SkeletonPlaceholder(props) {
  return h('div', { className: cx('cds--skeleton__placeholder', props.className), style: props.style });
}

// ------------------------------------------------------------------ Loading
function Loading(props) {
  var active = props.active === undefined ? true : props.active;
  var withOverlay = props.withOverlay === undefined ? true : props.withOverlay;
  var small = !!props.small;
  var desc = props.description || 'loading';
  var spinner = h('div', { id: props.id, 'aria-atomic': 'true', 'aria-live': active ? 'assertive' : 'off', className: cx(props.className, 'cds--loading', small && 'cds--loading--small', !active && 'cds--loading--stop') },
    h('svg', { className: 'cds--loading__svg', viewBox: '0 0 100 100' },
      h('title', null, desc),
      small ? h('circle', { className: 'cds--loading__background', cx: '50%', cy: '50%', r: '42' }) : null,
      h('circle', { className: 'cds--loading__stroke', cx: '50%', cy: '50%', r: small ? '42' : '44' })));
  return withOverlay ? h('div', { className: cx('cds--loading-overlay', !active && 'cds--loading-overlay--stop') }, spinner) : spinner;
}

function InlineLoading(props) {
  var status = props.status || 'active';
  var delay = props.successDelay === undefined ? 1500 : props.successDelay;
  var successRef = useRef(props.onSuccess); successRef.current = props.onSuccess;
  useEffect(function () {
    if (status !== 'finished') return;
    var t = setTimeout(function () { if (successRef.current) successRef.current(); }, delay);
    return function () { clearTimeout(t); };
  }, [status, delay]);
  var label = props.iconDescription;
  var anim = null;
  if (status === 'error') anim = B2Ico('error--filled', 16, 'cds--inline-loading--error', label || status, 16);
  else if (status === 'finished') anim = B2Ico('checkmark--filled', 16, 'cds--inline-loading__checkmark-container', label || status, 16);
  else if (status === 'active' || status === 'inactive') anim = h(Loading, { small: true, description: label || (status === 'active' ? 'loading' : 'not loading'), withOverlay: false, active: status === 'active' });
  return h('div', { className: cx('cds--inline-loading', props.className), 'aria-live': props['aria-live'] || 'assertive' },
    anim ? h('div', { className: 'cds--inline-loading__animation' }, anim) : null,
    props.description ? h('div', { className: 'cds--inline-loading__text' }, props.description) : null);
}

exports.Accordion = Accordion; exports.AccordionItem = AccordionItem;
exports.TreeView = TreeView; exports.TreeNode = TreeNode;
exports.CodeSnippet = CodeSnippet;
exports.ProgressBar = ProgressBar; exports.ProgressIndicator = ProgressIndicator; exports.ProgressStep = ProgressStep;
exports.SkeletonText = SkeletonText; exports.SkeletonPlaceholder = SkeletonPlaceholder;
exports.Loading = Loading; exports.InlineLoading = InlineLoading;

})(Carbon,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx);

/* ---- C1 ---- */
(function(exports,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx){
/* @components: Header, HeaderName, HeaderNavigation, HeaderMenuItem, HeaderGlobalBar, HeaderGlobalAction, HeaderMenuButton, SideNav, SideNavItems, SideNavLink, SideNavMenu, SideNavMenuItem, SideNavDivider, SkipToContent, Breadcrumb, BreadcrumbItem, ContentSwitcher, Switch, IconSwitch */

// ---------------------------------------------------------------- UI Shell
function Header(props) {
  var rest = {};
  for (var k in props) { if (k !== 'children' && k !== 'className') rest[k] = props[k]; }
  return h('header', Object.assign({}, rest, { className: cx('cds--header', props.className) }), props.children);
}
Header.displayName = 'Header';

function HeaderName(props) {
  var prefix = props.prefix === undefined ? 'IBM' : props.prefix;
  var rest = {};
  for (var k in props) { if (k !== 'children' && k !== 'className' && k !== 'prefix' && k !== 'element') rest[k] = props[k]; }
  if (!rest.href && !props.element) rest.href = '#';
  var El = props.element || 'a';
  return h(El, Object.assign({}, rest, { className: cx('cds--header__name', props.className) }),
    prefix ? h(React.Fragment, null, h('span', { className: 'cds--header__name--prefix' }, prefix), ' ') : null,
    props.children);
}
HeaderName.displayName = 'HeaderName';

function HeaderNavigation(props) {
  var rest = {};
  for (var k in props) { if (k !== 'children' && k !== 'className') rest[k] = props[k]; }
  return h('nav', Object.assign({}, rest, { className: cx('cds--header__nav', props.className) }),
    h('ul', { className: 'cds--header__menu-bar' }, props.children));
}
HeaderNavigation.displayName = 'HeaderNavigation';

function HeaderMenuItem(props) {
  var active = props.isActive || props.isCurrentPage;
  var ariaCurrent = props['aria-current'];
  var hasCurrent = active && ariaCurrent !== 'page';
  var rest = {};
  for (var k in props) { if (['children', 'className', 'isActive', 'isCurrentPage', 'aria-current', 'role', 'tabIndex', 'element'].indexOf(k) < 0) rest[k] = props[k]; }
  if (!rest.href && !props.element) rest.href = '#';
  var El = props.element || 'a';
  return h('li', { className: props.className, role: props.role },
    h(El, Object.assign({}, rest, {
      'aria-current': hasCurrent ? true : ariaCurrent,
      className: cx('cds--header__menu-item', hasCurrent && 'cds--header__menu-item--current'),
      tabIndex: props.tabIndex === undefined ? 0 : props.tabIndex
    }), h('span', { className: 'cds--text-truncate--end' }, props.children)));
}
HeaderMenuItem.displayName = 'HeaderMenuItem';

function HeaderGlobalBar(props) {
  return h('div', { className: cx('cds--header__global', props.className) }, props.children);
}
HeaderGlobalBar.displayName = 'HeaderGlobalBar';

function HeaderGlobalAction(props) {
  var label = props['aria-label'];
  var rest = {};
  for (var k in props) { if (['children', 'className', 'isActive', 'onClick', 'tooltipAlignment', 'tooltipDropShadow', 'tooltipHighContrast'].indexOf(k) < 0) rest[k] = props[k]; }
  return h('button', Object.assign({ type: 'button', title: label }, rest, {
    className: cx('cds--header__action', props.isActive && 'cds--header__action--active', props.className),
    onClick: props.onClick
  }), props.children);
}
HeaderGlobalAction.displayName = 'HeaderGlobalAction';

function HeaderMenuButton(props) {
  var label = props['aria-label'];
  var rest = {};
  for (var k in props) { if (['className', 'renderMenuIcon', 'renderCloseIcon', 'isActive', 'isCollapsible'].indexOf(k) < 0) rest[k] = props[k]; }
  var menuIcon = props.renderMenuIcon || h(Icon, { name: 'menu', size: 20 });
  var closeIcon = props.renderCloseIcon || h(Icon, { name: 'close', size: 20 });
  return h('button', Object.assign({}, rest, {
    type: 'button', title: label,
    className: cx(props.className, 'cds--header__action', 'cds--header__menu-trigger', props.isActive && 'cds--header__action--active', 'cds--header__menu-toggle', !props.isCollapsible && 'cds--header__menu-toggle__hidden')
  }), props.isActive ? closeIcon : menuIcon);
}
HeaderMenuButton.displayName = 'HeaderMenuButton';

function SkipToContent(props) {
  var rest = {};
  for (var k in props) { if (['children', 'className', 'href', 'tabIndex'].indexOf(k) < 0) rest[k] = props[k]; }
  return h('a', Object.assign({}, rest, {
    className: cx('cds--skip-to-content', props.className),
    href: props.href || '#main-content',
    tabIndex: props.tabIndex === undefined ? 0 : props.tabIndex
  }), props.children === undefined ? 'Skip to main content' : props.children);
}
SkipToContent.displayName = 'SkipToContent';

// ---- Side nav
var SideNavContext = React.createContext({ isRail: false, expanded: false });

function sideNavIcon(icon, small, extraClass) {
  var node = typeof icon === 'string' ? h(Icon, { name: icon, size: 32 }) : (typeof icon === 'function' ? h(icon) : icon);
  return h('div', { className: cx('cds--side-nav__icon', small && 'cds--side-nav__icon--small', extraClass) }, node);
}

function SideNav(props) {
  var controlled = props.expanded !== undefined;
  var st = useState(!!props.defaultExpanded);
  var hv = useState(false);
  var expanded = controlled ? props.expanded : st[0];
  var viaHover = hv[0];
  var isRail = !!props.isRail;
  var isFixed = !!props.isFixedNav;
  var isChildOfHeader = props.isChildOfHeader === undefined ? true : props.isChildOfHeader;
  var persistent = props.isPersistent === undefined ? true : props.isPersistent;
  var addFocus = props.addFocusListeners === undefined ? true : props.addFocusListeners;
  var addMouse = props.addMouseListeners === undefined ? true : props.addMouseListeners;
  var open = !!(expanded || viaHover);

  function toggle(event, value) {
    if (!controlled) st[1](value);
    if (controlled || isRail) hv[1](value);
    if (props.onToggle) props.onToggle(event, value);
  }
  var handlers = {};
  if (addFocus) {
    handlers.onFocus = function (e) { if (isRail && !e.currentTarget.contains(e.relatedTarget)) toggle(e, true); };
    handlers.onBlur = function (e) {
      if (!e.currentTarget.contains(e.relatedTarget)) {
        if (isRail || !isFixed) toggle(e, false);
        if (expanded && !isFixed && props.onSideNavBlur) props.onSideNavBlur();
      }
    };
    handlers.onKeyDown = function (e) {
      if (e.key === 'Escape') { toggle(e, false); if (props.href) window.location.href = props.href; }
    };
  }
  if (addMouse && isRail) {
    handlers.onMouseEnter = function () { toggle(true, true); };
    handlers.onMouseLeave = function () { st[1](false); hv[1](false); toggle(false, false); };
    handlers.onClick = function () { st[1](true); hv[1](true); toggle(true, true); };
  }
  var rest = {};
  for (var k in props) {
    if (['children', 'className', 'expanded', 'defaultExpanded', 'isChildOfHeader', 'onToggle', 'href', 'isFixedNav', 'isRail', 'isPersistent', 'addFocusListeners', 'addMouseListeners', 'onOverlayClick', 'onSideNavBlur', 'enterDelayMs'].indexOf(k) < 0) rest[k] = props[k];
  }
  var cls = cx('cds--side-nav__navigation', props.className, 'cds--side-nav',
    open && 'cds--side-nav--expanded',
    !expanded && isFixed && 'cds--side-nav--collapsed',
    isRail && 'cds--side-nav--rail',
    isChildOfHeader && 'cds--side-nav--ux',
    !persistent && 'cds--side-nav--hidden');
  return h(SideNavContext.Provider, { value: { isRail: isRail, expanded: open } },
    isFixed ? null : h('div', { className: cx('cds--side-nav__overlay', open && 'cds--side-nav__overlay-active'), onClick: props.onOverlayClick }),
    h('nav', Object.assign({ tabIndex: -1 }, rest, handlers, { className: cls }), props.children));
}
SideNav.displayName = 'SideNav';

function SideNavItems(props) {
  return h('ul', { className: cx('cds--side-nav__items', props.className) }, props.children);
}
SideNavItems.displayName = 'SideNavItems';

function useNavTabIndex(tabIndex, isSideNavExpanded) {
  var ctx = React.useContext(SideNavContext);
  var expanded = isSideNavExpanded === undefined ? ctx.expanded : isSideNavExpanded;
  return tabIndex === undefined ? (!expanded && !ctx.isRail ? -1 : 0) : tabIndex;
}

function SideNavLink(props) {
  var ti = useNavTabIndex(props.tabIndex, props.isSideNavExpanded);
  var rest = {};
  for (var k in props) { if (['children', 'className', 'renderIcon', 'isActive', 'isSideNavExpanded', 'large', 'tabIndex', 'element'].indexOf(k) < 0) rest[k] = props[k]; }
  if (!rest.href && !props.element) rest.href = '#';
  var El = props.element || 'a';
  return h('li', { className: cx('cds--side-nav__item', props.large && 'cds--side-nav__item--large') },
    h(El, Object.assign({}, rest, {
      className: cx('cds--side-nav__link', props.isActive && 'cds--side-nav__link--current', props.className),
      tabIndex: ti
    }), props.renderIcon ? sideNavIcon(props.renderIcon, true) : null,
      h('span', { className: 'cds--side-nav__link-text' }, props.children)));
}
SideNavLink.displayName = 'SideNavLink';

function hasActiveDescendant(children) {
  var kids = React.Children.toArray(children);
  for (var i = 0; i < kids.length; i++) {
    var c = kids[i];
    if (React.isValidElement(c) && (c.props.isActive === true || c.props['aria-current'])) return true;
  }
  return false;
}

function SideNavMenu(props) {
  var ctx = React.useContext(SideNavContext);
  var ex = useState(!!props.defaultExpanded);
  var prev = useState(!!props.defaultExpanded);
  var isExpanded = ex[0], setExpanded = ex[1], prevExpanded = prev[0], setPrev = prev[1];
  var navExpanded = props.isSideNavExpanded === undefined ? ctx.expanded : props.isSideNavExpanded;
  if (!navExpanded && isExpanded && ctx.isRail) { setExpanded(false); setPrev(true); }
  else if (navExpanded && prevExpanded && ctx.isRail) { setExpanded(true); setPrev(false); }
  var ti = props.tabIndex === undefined ? (!navExpanded && !ctx.isRail ? -1 : 0) : props.tabIndex;
  var active = props.isActive || (hasActiveDescendant(props.children) && !isExpanded);
  return h('li', {
    className: cx('cds--side-nav__item', active && 'cds--side-nav__item--active', props.renderIcon && 'cds--side-nav__item--icon', props.large && 'cds--side-nav__item--large', props.className),
    onKeyDown: function (e) { if (e.key === 'Escape') setExpanded(false); }
  },
    h('button', { type: 'button', className: 'cds--side-nav__submenu', 'aria-expanded': isExpanded, tabIndex: ti, onClick: function () { setExpanded(!isExpanded); } },
      props.renderIcon ? sideNavIcon(props.renderIcon, false) : null,
      h('span', { className: 'cds--side-nav__submenu-title' }, props.title),
      h('div', { className: 'cds--side-nav__icon cds--side-nav__icon--small cds--side-nav__submenu-chevron' }, h(Icon, { name: 'chevron--down', size: 16 }))),
    h('ul', { className: 'cds--side-nav__menu' }, props.children));
}
SideNavMenu.displayName = 'SideNavMenu';

function SideNavMenuItem(props) {
  var rest = {};
  for (var k in props) { if (['children', 'className', 'isActive', 'element', 'as'].indexOf(k) < 0) rest[k] = props[k]; }
  if (!rest.href && !props.element && !props.as) rest.href = '#';
  var El = props.as || props.element || 'a';
  return h('li', { className: cx('cds--side-nav__menu-item', props.className) },
    h(El, Object.assign({}, rest, { className: cx('cds--side-nav__link', props.isActive && 'cds--side-nav__link--current') }),
      h('span', { className: 'cds--side-nav__link-text' }, props.children)));
}
SideNavMenuItem.displayName = 'SideNavMenuItem';

function SideNavDivider(props) {
  return h('li', { className: cx('cds--side-nav__divider', props.className) }, h('hr'));
}
SideNavDivider.displayName = 'SideNavDivider';

// ---------------------------------------------------------------- Breadcrumb
function Breadcrumb(props) {
  var rest = {};
  for (var k in props) { if (['aria-label', 'children', 'className', 'noTrailingSlash', 'size'].indexOf(k) < 0) rest[k] = props[k]; }
  return h('nav', Object.assign({ className: props.className, 'aria-label': props['aria-label'] || 'Breadcrumb' }, rest),
    h('ol', { className: cx('cds--breadcrumb', props.noTrailingSlash && 'cds--breadcrumb--no-trailing-slash', props.size === 'sm' && 'cds--breadcrumb--sm') }, props.children));
}
Breadcrumb.displayName = 'Breadcrumb';

function BreadcrumbItem(props) {
  var ariaCurrent = props['aria-current'];
  var rest = {};
  for (var k in props) { if (['aria-current', 'children', 'className', 'href', 'isCurrentPage'].indexOf(k) < 0) rest[k] = props[k]; }
  var cls = cx('cds--breadcrumb-item', props.isCurrentPage && ariaCurrent !== 'page' && 'cds--breadcrumb-item--current', props.className);
  var cur = ariaCurrent || (props.isCurrentPage ? true : undefined);
  var inner;
  var kid = props.children;
  var kt = React.isValidElement(kid) ? kid.type : null;
  if (kt && (kt === exports.OverflowMenu || /OverflowMenu/.test(kt.displayName || kt.name || ''))) {
    // Overflow crumb: an OverflowMenu of the hidden levels, drawn as a link-colored horizontal ellipsis.
    return h('li', Object.assign({ className: cx('cds--breadcrumb-item', props.className) }, rest),
      React.cloneElement(kid, {
        renderIcon: kid.props.renderIcon || 'overflow-menu--horizontal',
        size: kid.props.size || 'sm',
        iconDescription: kid.props.iconDescription || 'Show more breadcrumbs',
        className: cx('cds--breadcrumb-overflow', kid.props.className)
      }));
  }
  if (typeof props.children === 'string') {
    inner = props.href
      ? h('a', { className: 'cds--link', href: props.href, 'aria-current': cur }, props.children)
      : h('span', { className: 'cds--link', 'aria-current': cur }, props.children);
  } else if (React.isValidElement(props.children)) {
    inner = React.cloneElement(props.children, { 'aria-current': ariaCurrent, className: cx('cds--link', props.children.props.className) });
  } else {
    inner = props.children;
  }
  return h('li', Object.assign({ className: cls }, rest), inner);
}
BreadcrumbItem.displayName = 'BreadcrumbItem';

// ---------------------------------------------------------------- ContentSwitcher
function Switch(props) {
  var rest = {};
  for (var k in props) { if (['children', 'className', 'disabled', 'index', 'name', 'onClick', 'onKeyDown', 'selected', 'text', 'forwardedRef', 'size'].indexOf(k) < 0) rest[k] = props[k]; }
  var sel = !!props.selected;
  function click(e) { e.preventDefault(); if (props.onClick) props.onClick({ index: props.index, name: props.name, text: props.text }); }
  function key(e) { if (props.onKeyDown) props.onKeyDown({ index: props.index, name: props.name, text: props.text, key: e.key }); }
  return h('button', Object.assign({ type: 'button', role: 'tab', tabIndex: sel ? 0 : -1, 'aria-selected': sel }, rest, {
    ref: props.forwardedRef, onClick: click, onKeyDown: key, disabled: props.disabled,
    className: cx(props.className, 'cds--content-switcher-btn', sel && 'cds--content-switcher--selected')
  }), h('span', { className: 'cds--content-switcher__label', title: props.text }, props.text !== undefined ? props.text : props.children));
}
Switch.displayName = 'Switch';

// Icon-only option. Carbon renders an IconButton with a tooltip; here the label is the accessible name and native title.
function IconSwitch(props) {
  var rest = {};
  for (var k in props) { if (['children', 'className', 'disabled', 'index', 'name', 'onClick', 'onKeyDown', 'selected', 'text', 'forwardedRef', 'size', 'renderIcon', 'align', 'enterDelayMs', 'leaveDelayMs'].indexOf(k) < 0) rest[k] = props[k]; }
  var sel = !!props.selected;
  function click(e) { e.preventDefault(); if (props.onClick) props.onClick({ index: props.index, name: props.name, text: props.text }); }
  function key(e) { if (props.onKeyDown) props.onKeyDown({ index: props.index, name: props.name, text: props.text, key: e.key }); }
  var icon = props.renderIcon || props.children;
  var glyph = typeof icon === 'string' ? h(Icon, { name: icon, size: 16 }) : (typeof icon === 'function' ? h(icon, { size: 16 }) : icon);
  return h('button', Object.assign({ type: 'button', role: 'tab', tabIndex: sel ? 0 : -1, 'aria-selected': sel, 'aria-label': props.text, title: props.text }, rest, {
    ref: props.forwardedRef, onClick: click, onKeyDown: key, disabled: props.disabled,
    className: cx(props.className, 'cds--content-switcher-btn', 'cds--content-switcher__icon-btn', sel && 'cds--content-switcher--selected')
  }), h('span', { className: 'cds--content-switcher__icon' }, glyph));
}
IconSwitch.displayName = 'IconSwitch';

function ContentSwitcher(props) {
  var size = props.size || 'md';
  var mode = props.selectionMode || 'automatic';
  var st = useState(props.selectedIndex || 0);
  var selected = st[0], setSelected = st[1];
  var prev = useRef(props.selectedIndex || 0);
  var refs = useRef([]);
  var kids = React.Children.toArray(props.children);
  useEffect(function () {
    if (prev.current !== props.selectedIndex && props.selectedIndex !== undefined) { setSelected(props.selectedIndex); prev.current = props.selectedIndex; }
  }, [props.selectedIndex]);
  function focusAt(i) { var r = refs.current[i]; if (r) r.focus(); }
  function change(p) { if (props.onChange) props.onChange(p); }
  function next(from, dir) {
    var n = kids.length; var i = from;
    for (var c = 0; c < n; c++) { i = (i + dir + n) % n; if (!kids[i].props.disabled) return i; }
    return from;
  }
  function onItem(e) {
    if (typeof e.index === 'undefined') return;
    var index = e.index;
    if (e.key !== undefined && (e.key === 'ArrowRight' || e.key === 'ArrowLeft')) {
      var ni = next(index, e.key === 'ArrowRight' ? 1 : -1);
      focusAt(ni);
      if (mode !== 'manual') {
        setSelected(ni);
        change({ index: ni, name: kids[ni].props.name, text: kids[ni].props.text, key: e.key });
      }
    } else if (selected !== index && (e.key === undefined || e.key === 'Enter' || e.key === ' ' || e.key === 'Spacebar')) {
      setSelected(index); focusAt(index); change(e);
    }
  }
  var rest = {};
  for (var k in props) { if (['children', 'className', 'selectedIndex', 'selectionMode', 'size', 'onChange', 'light', 'lowContrast'].indexOf(k) < 0) rest[k] = props[k]; }
  var iconOnly = kids.length > 0 && kids.every(function (c) { return React.isValidElement(c) && c.type === IconSwitch; });
  return h('div', Object.assign({}, rest, { role: 'tablist', className: cx('cds--content-switcher', 'cds--content-switcher--' + size, 'cds--layout--size-' + size, iconOnly && 'cds--content-switcher--icon-only', props.lowContrast && 'cds--content-switcher--low-contrast', props.className) }),
    kids.map(function (child, i) {
      return React.cloneElement(child, {
        key: i, index: i, size: size, selected: i === selected,
        onClick: function (e) { onItem(e); if (child.props.onClick) child.props.onClick(e); },
        onKeyDown: function (e) { onItem(e); if (child.props.onKeyDown) child.props.onKeyDown(e); },
        forwardedRef: function (r) { if (r) refs.current[i] = r; }
      });
    }));
}
ContentSwitcher.displayName = 'ContentSwitcher';

exports.Header = Header; exports.HeaderName = HeaderName; exports.HeaderNavigation = HeaderNavigation;
exports.HeaderMenuItem = HeaderMenuItem; exports.HeaderGlobalBar = HeaderGlobalBar; exports.HeaderGlobalAction = HeaderGlobalAction;
exports.HeaderMenuButton = HeaderMenuButton; exports.SkipToContent = SkipToContent;
exports.SideNav = SideNav; exports.SideNavItems = SideNavItems; exports.SideNavLink = SideNavLink;
exports.SideNavMenu = SideNavMenu; exports.SideNavMenuItem = SideNavMenuItem; exports.SideNavDivider = SideNavDivider;
exports.Breadcrumb = Breadcrumb; exports.BreadcrumbItem = BreadcrumbItem;
exports.ContentSwitcher = ContentSwitcher; exports.Switch = Switch; exports.IconSwitch = IconSwitch;

})(Carbon,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx);

/* ---- C2 ---- */
(function(exports,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx){
/* @components: Menu, MenuItem, MenuItemDivider, MenuItemGroup, MenuItemSelectable, MenuItemRadioGroup, MenuButton, OverflowMenu, ComboButton */
// Shared menu implementation for Menu, MenuButton, OverflowMenu and ComboButton (group C2).
var MenuCtx = React.createContext(null);

function useControllable(value, defaultValue, onChange) {
  var st = useState(defaultValue); var inner = st[0], setInner = st[1];
  var controlled = value !== undefined;
  var cur = controlled ? value : inner;
  function set(v) { if (!controlled) setInner(v); if (onChange) onChange(v); }
  return [cur, set];
}

function menuItemsOf(ul) {
  return Array.prototype.filter.call(ul.querySelectorAll('[role^="menuitem"]'), function (el) {
    return el.closest('[role="menu"]') === ul && el.getAttribute('aria-disabled') !== 'true';
  });
}

function menuRenderIcon(icon, size) {
  if (!icon) return null;
  if (typeof icon === 'string') return h(Icon, { name: icon, size: size || 16 });
  if (React.isValidElement(icon)) return icon;
  // A component: a function, or a forwardRef/memo object (they carry $typeof).
  if (typeof icon === 'function' || (typeof icon === 'object' && icon.$typeof)) return h(icon, null);
  return icon;
}

// Menu -------------------------------------------------------------------
function Menu(props) {
  var parent = React.useContext(MenuCtx);
  var isRoot = !parent;
  var size = isRoot ? (props.size || 'sm') : parent.size;
  var open = !!props.open;
  var onClose = props.onClose;
  var s1 = useState(false), hasIcons = s1[0], setHasIcons = s1[1];
  var s2 = useState(false), hasSel = s2[0], setHasSel = s2[1];
  var ref = useRef(null);
  var focusReturn = useRef(null);
  var mounted = useRef(false);

  function returnFocus() {
    var t = props.returnFocusRef && props.returnFocusRef.current;
    if (t && t.focus) t.focus();
    else if (focusReturn.current && focusReturn.current.focus) focusReturn.current.focus();
  }
  function handleClose() {
    returnFocus();
    if (onClose) onClose();
  }
  var ctx = {
    size: size,
    requestCloseRoot: isRoot ? handleClose : parent.requestCloseRoot,
    enableIcons: function () { setHasIcons(true); },
    enableSelectable: function () { setHasSel(true); }
  };

  useEffect(function () {
    var first = !mounted.current;
    mounted.current = true;
    if (!open) return undefined;
    focusReturn.current = document.activeElement;
    if (props.onOpen) props.onOpen();
    if (!isRoot || first) return undefined;
    var raf = null;
    if (ref.current) {
      ref.current.focus();
      raf = requestAnimationFrame(function () {
        if (!ref.current) return;
        var items = menuItemsOf(ref.current);
        if (items.length) items[0].focus();
      });
    }
    return function () { if (raf) cancelAnimationFrame(raf); };
  }, [open]);

  function onKeyDown(e) {
    e.stopPropagation();
    var k = e.key;
    if ((k === 'Escape' || (!isRoot && k === 'ArrowLeft')) && onClose) { e.preventDefault(); handleClose(); return; }
    var ul = ref.current; if (!ul) return;
    var items = menuItemsOf(ul); if (!items.length) return;
    var cur = -1;
    for (var i = 0; i < items.length; i++) { if (items[i] === document.activeElement || items[i].contains(document.activeElement)) { cur = i; break; } }
    var next = cur;
    if (k === 'ArrowDown') next = cur < 0 ? 0 : (cur + 1) % items.length;
    else if (k === 'ArrowUp') next = cur < 0 ? items.length - 1 : (cur - 1 + items.length) % items.length;
    else if (k === 'Home') next = 0;
    else if (k === 'End') next = items.length - 1;
    else return;
    e.preventDefault();
    items[next].focus();
  }

  function onBlur(e) {
    if (!open || !onClose || !isRoot) return;
    var rt = e.relatedTarget;
    if (!rt || (ref.current && ref.current.contains(rt))) return;
    var c = props.containerRef && props.containerRef.current;
    if (c && c.contains(rt)) return;
    if (onClose) onClose();
  }

  if (!open) return null;

  var x = props.x, y = props.y;
  var style = Object.assign({}, props.style);
  if (isRoot) {
    var xv = Array.isArray(x) ? x[0] : x, yv = Array.isArray(y) ? y[0] : y;
    if (typeof xv === 'number') style.insetInlineStart = xv;
    if (typeof yv === 'number') style.insetBlockStart = yv;
  }
  var cls = cx(props.className, 'cds--menu', 'cds--menu--' + size,
    open && 'cds--menu--open', open && 'cds--menu--shown',
    isRoot && props.fixed && 'cds--menu--fixed',
    isRoot && props.menuAlignment && 'cds--menu--align-' + props.menuAlignment,
    hasIcons && 'cds--menu--with-icons', hasSel && 'cds--menu--with-selectable-items',
    props.border && 'cds--menu--border', props.backgroundToken === 'background' && 'cds--menu--background-token__background');
  return h(MenuCtx.Provider, { value: ctx },
    h('ul', { id: props.id, className: cls, role: 'menu', ref: ref, 'aria-label': props.label, tabIndex: -1, style: style, onKeyDown: onKeyDown, onBlur: onBlur }, props.children));
}

// MenuItem ---------------------------------------------------------------
function MenuItem(props) {
  var ctx = React.useContext(MenuCtx) || {};
  var label = props.label, kind = props.kind || 'default';
  var hasChildren = !!props.children;
  var isDisabled = !!props.disabled && !hasChildren;
  var isDanger = kind === 'danger' && !hasChildren;
  var st = useState(!!props.defaultSubmenuOpen), subOpen = st[0], setSubOpen = st[1];
  var li = useRef(null);
  var timer = useRef(null);
  var pendingKey = useRef(false);
  var checked = props['aria-checked'];

  useEffect(function () { if (props.renderIcon && ctx.enableIcons) ctx.enableIcons(); }, [props.renderIcon]);
  useEffect(function () { return function () { clearTimeout(timer.current); }; }, []);

  function fromSubmenu(e) {
    return hasChildren && li.current && e.target !== li.current && li.current.querySelector('[role="menu"]') && li.current.querySelector('[role="menu"]').contains(e.target);
  }
  function click(e) {
    if (fromSubmenu(e) || isDisabled) return;
    if (hasChildren) { setSubOpen(true); return; }
    if (ctx.requestCloseRoot) ctx.requestCloseRoot(e);
    if (props.onClick) props.onClick(e);
  }
  function keyDown(e) {
    if (e.target === li.current && hasChildren && !props.disabled && e.key === 'ArrowRight') {
      setSubOpen(true);
      setTimeout(function () {
        var sub = li.current && li.current.querySelector('[role="menu"]');
        if (sub) { var it = menuItemsOf(sub); if (it.length) it[0].focus(); }
      }, 0);
      e.stopPropagation(); e.preventDefault();
    }
    pendingKey.current = e.target === li.current && (e.key === 'Enter' || e.key === ' ');
    if (pendingKey.current && e.key === ' ') e.preventDefault();
    if (props.onKeyDown) props.onKeyDown(e);
  }
  function keyUp(e) {
    if (pendingKey.current && (e.key === 'Enter' || e.key === ' ')) click(e);
    pendingKey.current = false;
  }
  function enter() {
    if (!hasChildren || props.disabled) return;
    clearTimeout(timer.current);
    timer.current = setTimeout(function () { setSubOpen(true); }, 100);
  }
  function leave() {
    if (!hasChildren) return;
    clearTimeout(timer.current);
    timer.current = setTimeout(function () { setSubOpen(false); }, 100);
  }

  var cls = cx(props.className, 'cds--menu-item', isDisabled && 'cds--menu-item--disabled', isDanger && 'cds--menu-item--danger', hasChildren && 'cds--menu-item--has-submenu');
  return h('li', {
    id: props.id, role: props.role || 'menuitem', ref: li, className: cls,
    tabIndex: -1,
    'aria-disabled': isDisabled || undefined,
    'aria-haspopup': hasChildren ? 'menu' : undefined,
    'aria-expanded': hasChildren ? subOpen : undefined,
    'aria-checked': checked,
    title: label,
    onClick: click, onKeyDown: keyDown, onKeyUp: keyUp, onMouseEnter: enter, onMouseLeave: leave
  },
    h('div', { className: 'cds--menu-item__selection-icon' }, checked ? h(Icon, { name: 'checkmark', size: 16 }) : null),
    h('div', { className: 'cds--menu-item__icon' }, menuRenderIcon(props.renderIcon)),
    h('div', { className: 'cds--menu-item__label' }, label),
    props.shortcut && !hasChildren ? h('div', { className: 'cds--menu-item__shortcut' }, props.shortcut) : null,
    hasChildren ? h('div', { className: 'cds--menu-item__shortcut' }, h(Icon, { name: 'caret--right', size: 16 })) : null,
    hasChildren ? h(Menu, { label: label, open: subOpen, onClose: function () { setSubOpen(false); if (li.current) li.current.focus(); } }, props.children) : null);
}

// MenuItemSelectable -----------------------------------------------------
function MenuItemSelectable(props) {
  var ctx = React.useContext(MenuCtx) || {};
  var sel = useControllable(props.selected, props.defaultSelected || false, props.onChange);
  useEffect(function () { if (ctx.enableSelectable) ctx.enableSelectable(); }, []);
  return h(MenuItem, {
    label: props.label, className: props.className, disabled: props.disabled, shortcut: props.shortcut, renderIcon: props.renderIcon, id: props.id,
    role: 'menuitemcheckbox', 'aria-checked': sel[0], onClick: function () { sel[1](!sel[0]); }
  });
}

// MenuItemGroup ----------------------------------------------------------
function MenuItemGroup(props) {
  return h('li', { className: cx('cds--menu-item-group', props.className), role: 'none' },
    h('ul', { role: 'group', 'aria-label': props.label }, props.children));
}

// MenuItemRadioGroup -----------------------------------------------------
function MenuItemRadioGroup(props) {
  var ctx = React.useContext(MenuCtx) || {};
  var items = props.items || [];
  var toStr = props.itemToString || function (it) { return it == null ? '' : (typeof it === 'string' ? it : (it.label || String(it))); };
  var sel = useControllable(props.selectedItem, props.defaultSelectedItem, props.onChange);
  useEffect(function () { if (ctx.enableSelectable) ctx.enableSelectable(); }, []);
  return h('li', { className: cx('cds--menu-item-radio-group', props.className), role: 'none' },
    h('ul', { role: 'group', 'aria-label': props.label },
      items.map(function (it, i) {
        return h(MenuItem, { key: i, label: toStr(it), role: 'menuitemradio', 'aria-checked': it === sel[0], onClick: function () { sel[1](it); } });
      })));
}

// MenuItemDivider --------------------------------------------------------
function MenuItemDivider(props) {
  return h('li', { className: cx('cds--menu-item-divider', props.className), role: 'separator' });
}

// Shared open/close state for the attached-menu components -----------------
function useAttachedMenu(props, wrapRef) {
  var st = useState(!!props.defaultOpen);
  var controlled = props.open !== undefined;
  var open = controlled ? !!props.open : st[0];
  var latest = useRef(null);
  latest.current = { controlled: controlled, props: props, setInner: st[1] };
  function set(v) {
    var l = latest.current;
    if (!l.controlled) l.setInner(v);
    if (v) { if (l.props.onOpen) l.props.onOpen(); } else if (l.props.onClose) l.props.onClose();
  }
  useEffect(function () {
    if (!open) return undefined;
    function down(e) { if (wrapRef.current && !wrapRef.current.contains(e.target)) set(false); }
    document.addEventListener('mousedown', down);
    return function () { document.removeEventListener('mousedown', down); };
  }, [open]);
  return { open: open, toggle: function () { set(!open); }, close: function () { set(false); } };
}

// MenuButton -------------------------------------------------------------
function MenuButton(props) {
  var id = 'menubtn-' + useId();
  var wrap = useRef(null), trig = useRef(null);
  var am = useAttachedMenu(props, wrap);
  var kind = props.kind || 'primary', size = props.size || 'lg', align = props.menuAlignment || 'bottom';
  return h('div', { ref: wrap, className: cx('cds--menu-button__container', props.className), style: props.style },
    h('button', {
      ref: trig, type: 'button', disabled: props.disabled, 'aria-haspopup': 'menu', 'aria-expanded': am.open, 'aria-controls': am.open ? id : undefined,
      className: cx('cds--btn', 'cds--btn--' + kind, 'cds--btn--' + size, 'cds--menu-button__trigger', am.open && 'cds--menu-button__trigger--open'),
      onClick: am.toggle
    }, props.label, h(Icon, { name: 'chevron--down', size: 16, className: 'cds--btn__icon' })),
    h(Menu, {
      id: id, label: props.label, size: size, open: am.open, onClose: am.close, containerRef: wrap, returnFocusRef: trig, menuAlignment: align,
      className: 'cds--menu-button__' + align, border: props.menuBorder, backgroundToken: props.menuBackgroundToken
    }, props.children));
}

// OverflowMenu -----------------------------------------------------------
function OverflowMenu(props) {
  var id = 'overflow-' + useId();
  var wrap = useRef(null), trig = useRef(null);
  var am = useAttachedMenu(props, wrap);
  var size = props.size || 'md';
  var desc = props.iconDescription || props.label || props['aria-label'] || 'Options';
  var dir = props.direction === 'top' ? 'top' : 'bottom';
  var align = dir + (props.flipped ? '-end' : '-start');
  var icon = props.renderIcon || 'overflow-menu--vertical';
  return h('div', { ref: wrap, className: cx('cds--overflow-menu__wrapper', props.className), style: props.style },
    h('button', {
      ref: trig, type: 'button', title: desc, 'aria-label': desc, disabled: props.disabled, 'aria-haspopup': 'menu', 'aria-expanded': am.open, 'aria-controls': am.open ? id : undefined,
      className: cx('cds--overflow-menu', 'cds--overflow-menu--' + size, am.open && 'cds--overflow-menu--open', props.flipped && 'cds--overflow-menu--flip'),
      onClick: am.toggle
    }, typeof icon === 'string' ? h(Icon, { name: icon, size: 16, className: 'cds--overflow-menu__icon' }) : h('span', { className: 'cds--overflow-menu__icon' }, menuRenderIcon(icon))),
    h(Menu, { id: id, label: desc, size: size, open: am.open, onClose: am.close, containerRef: wrap, returnFocusRef: trig, menuAlignment: align, className: 'cds--overflow-menu__menu' }, props.children));
}

// ComboButton ------------------------------------------------------------
function ComboButton(props) {
  var id = 'combobtn-' + useId();
  var wrap = useRef(null), trig = useRef(null);
  var am = useAttachedMenu(props, wrap);
  var size = props.size || 'lg', align = props.menuAlignment || 'bottom';
  return h('div', { ref: wrap, className: cx('cds--combo-button__container', 'cds--combo-button__container--' + size, am.open && 'cds--combo-button__container--open', props.className), style: props.style },
    h('div', { className: 'cds--combo-button__primary-action' },
      h('button', { type: 'button', title: props.label, disabled: props.disabled, className: cx('cds--btn', 'cds--btn--primary', 'cds--btn--' + size), onClick: props.onClick }, props.label)),
    h('button', {
      ref: trig, type: 'button', title: 'Additional actions', 'aria-label': 'Additional actions', disabled: props.disabled, 'aria-haspopup': 'menu', 'aria-expanded': am.open, 'aria-controls': am.open ? id : undefined,
      className: cx('cds--btn', 'cds--btn--primary', 'cds--btn--icon-only', 'cds--btn--' + size, 'cds--combo-button__trigger'),
      onClick: am.toggle
    }, h(Icon, { name: 'chevron--down', size: 16, className: 'cds--btn__icon' })),
    h(Menu, { id: id, label: 'Additional actions', size: size, open: am.open, onClose: am.close, containerRef: wrap, returnFocusRef: trig, menuAlignment: align, className: 'cds--combo-button__' + align }, props.children));
}

Menu.displayName = 'Menu'; MenuItem.displayName = 'MenuItem'; MenuItemDivider.displayName = 'MenuItemDivider';
MenuItemGroup.displayName = 'MenuItemGroup'; MenuItemSelectable.displayName = 'MenuItemSelectable'; MenuItemRadioGroup.displayName = 'MenuItemRadioGroup';
MenuButton.displayName = 'MenuButton'; OverflowMenu.displayName = 'OverflowMenu'; ComboButton.displayName = 'ComboButton';

exports.Menu = Menu;
exports.MenuItem = MenuItem;
exports.MenuItemDivider = MenuItemDivider;
exports.MenuItemGroup = MenuItemGroup;
exports.MenuItemSelectable = MenuItemSelectable;
exports.MenuItemRadioGroup = MenuItemRadioGroup;
exports.MenuButton = MenuButton;
exports.OverflowMenu = OverflowMenu;
exports.ComboButton = ComboButton;

})(Carbon,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx);

/* ---- D1 ---- */
(function(exports,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx){
/* @components: Modal, ComposedModal, ModalHeader, ModalBody, ModalFooter, ToastNotification, ActionableNotification */

// ---------------------------------------------------------------------------
// Shared helpers
// ---------------------------------------------------------------------------
var D1_FOCUSABLE = 'a[href],button:not([disabled]),input:not([disabled]):not([type="hidden"]),select:not([disabled]),textarea:not([disabled]),[tabindex]:not([tabindex="-1"])';
function d1Focusable(root) {
  if (!root) return [];
  var all = root.querySelectorAll(D1_FOCUSABLE), out = [];
  for (var i = 0; i < all.length; i++) {
    var el = all[i];
    if (el.getAttribute('aria-hidden') === 'true') continue;
    if (el.offsetWidth || el.offsetHeight || el.getClientRects().length) out.push(el);
  }
  return out;
}
function d1Noop() {}

// ---------------------------------------------------------------------------
// Modal core: scrim + container, focus trap, Escape, return focus
// ---------------------------------------------------------------------------
var D1ModalContext = React.createContext(null);

function ModalCore(props) {
  var open = !!props.open, inline = !!props.inline;
  var rootRef = useRef(null), containerRef = useRef(null);
  var prevOpen = useRef(false), lastFocus = useRef(null);
  var onClose = useRef(props.onRequestClose);
  onClose.current = props.onRequestClose;

  // Escape closes (document level, capture, like Carbon).
  useEffect(function () {
    if (!open) return;
    function onKey(e) {
      if (e.key === 'Escape' || e.key === 'Esc') {
        var c = containerRef.current;
        if (inline && c && !c.contains(document.activeElement) && document.activeElement !== document.body) return;
        e.preventDefault(); e.stopPropagation();
        if (onClose.current) onClose.current(e);
      }
    }
    document.addEventListener('keydown', onKey, true);
    return function () { document.removeEventListener('keydown', onKey, true); };
  }, [open, inline]);

  // Body scroll lock (real, fixed use only).
  useEffect(function () {
    if (inline) return;
    document.body.classList.toggle('cds--body--with-modal-open', open);
    return function () { document.body.classList.remove('cds--body--with-modal-open'); };
  }, [open, inline]);

  // Initial focus and focus return.
  useEffect(function () {
    var c = containerRef.current;
    if (open && !prevOpen.current) {
      var ae = document.activeElement;
      if (ae && c && !c.contains(ae) && ae !== document.body) lastFocus.current = ae;
      if (!inline && c) {
        var sel = props.selectorPrimaryFocus || '[data-modal-primary-focus]';
        var target = c.querySelector(sel);
        if (!target && props.danger) target = c.querySelector('.cds--modal-footer .cds--btn--secondary:not([disabled])');
        // Carbon guidance: first form field in the body, else the primary (last enabled) footer button.
        if (!target) target = c.querySelector('.cds--modal-content input:not([disabled]):not([type="hidden"]), .cds--modal-content select:not([disabled]), .cds--modal-content textarea:not([disabled])');
        if (!target) {
          var btns = c.querySelectorAll('.cds--modal-footer .cds--btn:not([disabled])');
          if (btns.length) target = btns[btns.length - 1];
        }
        if (!target) target = c.querySelector('.cds--modal-close');
        if (!target) target = c;
        target.focus();
      }
    } else if (!open && prevOpen.current) {
      var l = props.launcherButtonRef && props.launcherButtonRef.current;
      var back = l || lastFocus.current;
      if (back && back.focus) setTimeout(function () { back.focus(); });
      lastFocus.current = null;
    }
    prevOpen.current = open;
  }, [open]);

  // Keep focus inside while open (Tab wrap + stray focus).
  useEffect(function () {
    if (!open || inline) return;
    function onFocusIn(e) {
      var c = containerRef.current;
      if (c && !c.contains(e.target)) {
        var f = d1Focusable(c);
        (f[0] || c).focus();
      }
    }
    document.addEventListener('focusin', onFocusIn);
    return function () { document.removeEventListener('focusin', onFocusIn); };
  }, [open, inline]);

  function onKeyDown(e) {
    if (props.onKeyDown) props.onKeyDown(e);
    if (!open) return;
    if (e.key === 'Tab') {
      var f = d1Focusable(containerRef.current);
      if (!f.length) { e.preventDefault(); return; }
      var first = f[0], last = f[f.length - 1], ae = document.activeElement;
      if (e.shiftKey && (ae === first || ae === containerRef.current)) { e.preventDefault(); last.focus(); }
      else if (!e.shiftKey && ae === last) { e.preventDefault(); first.focus(); }
    } else if (e.key === 'Enter' && props.shouldSubmitOnEnter && props.onRequestSubmit) {
      var t = e.target;
      if (t && t.tagName !== 'BUTTON' && t.tagName !== 'TEXTAREA' && t.tagName !== 'A') props.onRequestSubmit(e);
    }
  }

  function onScrimClick(e) {
    var c = containerRef.current;
    if (props.closeOnOutsideClick && c && !c.contains(e.target) && props.onRequestClose) props.onRequestClose(e);
  }

  var overlay = h('div', {
    ref: rootRef, onKeyDown: onKeyDown, onClick: onScrimClick,
    className: cx('cds--modal', open && 'is-visible', props.danger && 'cds--modal--danger', inline && 'cds--modal--inline', props.className)
  },
    h('div', Object.assign({
      ref: containerRef, role: props.alertdialog ? 'alertdialog' : 'dialog', 'aria-modal': 'true', tabIndex: -1,
      'aria-label': props.ariaLabel, 'aria-labelledby': props.ariaLabel ? undefined : props.labelledBy,
      'aria-describedby': props.alertdialog ? props.describedBy : undefined,
      className: cx('cds--modal-container', props.size && 'cds--modal-container--' + props.size, props.isFullWidth && 'cds--modal-container--full-width', props.containerClassName)
    }), props.children));

  if (inline) {
    return h('div', { className: 'cds--modal-preview-frame', style: { blockSize: props.inlineHeight || '24rem' } }, overlay);
  }
  return overlay;
}

function ModalCloseButton(props) {
  var label = props.label || 'Close';
  return h('div', { className: 'cds--modal-close-button' },
    h('button', { type: 'button', className: 'cds--modal-close', title: label, 'aria-label': label, onClick: props.onClick },
      h(Icon, { name: 'close', size: 20, className: 'cds--modal-close__icon' })));
}

function ModalContentArea(props) {
  var ref = useRef(null);
  var st = useState({ scroll: false, tall: false }); var s = st[0], setS = st[1];
  useLayoutEffect(function () {
    var el = ref.current; if (!el) return;
    var scroll = el.scrollHeight > el.clientHeight, tall = el.clientHeight > 300;
    if (scroll !== s.scroll || tall !== s.tall) setS({ scroll: scroll, tall: tall });
  });
  var scrolling = props.hasScrollingContent || s.scroll;
  var extra = scrolling ? { tabIndex: 0, role: 'region', 'aria-label': props.ariaLabel, 'aria-labelledby': props.ariaLabel ? undefined : props.labelledBy } : {};
  return h('div', Object.assign({
    ref: ref, id: props.id,
    className: cx('cds--modal-content', props.hasForm && 'cds--modal-content--with-form', scrolling && 'cds--modal-scroll-content', !s.tall && 'cds--modal-scroll-content--no-fade', props.className)
  }, extra), props.children);
}

// ---------------------------------------------------------------------------
// Modal
// ---------------------------------------------------------------------------
function Modal(props) {
  var uid = useId();
  var labelId = 'cds-modal-label-' + uid, headingId = 'cds-modal-heading-' + uid, bodyId = 'cds-modal-body-' + uid;
  var passive = !!props.passiveModal, danger = !!props.danger;
  var onClose = props.onRequestClose || d1Noop, onSubmit = props.onRequestSubmit || d1Noop;
  var inline = props.inline || props.static;
  var strOrU = function (n) { return typeof n === 'string' ? n : undefined; };
  var ariaLabel = strOrU(props.modalLabel) || props['aria-label'] || props.modalAriaLabel || strOrU(props.modalHeading);

  var footer = null;
  if (!passive) {
    var sbs = props.secondaryButtons, btns = [];
    if (Array.isArray(sbs) && sbs.length <= 2 && sbs.length > 0) {
      sbs.forEach(function (b, i) { btns.push(h(exports.Button, { key: 's' + i, kind: 'secondary', onClick: b.onClick }, b.buttonText)); });
    } else if (props.secondaryButtonText) {
      btns.push(h(exports.Button, { key: 's', kind: 'secondary', onClick: props.onSecondarySubmit || onClose }, props.secondaryButtonText));
    }
    var loadingStatus = props.loadingStatus || 'inactive', loadingActive = loadingStatus !== 'inactive';
    var IL = exports.InlineLoading;
    if (loadingActive) btns = btns.map(function (b) { return React.cloneElement(b, { disabled: true }); });
    btns.push(h(exports.Button, { key: 'p', kind: danger ? 'danger' : 'primary', disabled: loadingActive || props.primaryButtonDisabled, onClick: onSubmit },
      loadingActive && IL
        ? h(IL, { status: loadingStatus, description: props.loadingDescription, iconDescription: props.loadingIconDescription, className: 'cds--inline-loading--btn', onSuccess: props.onLoadingSuccess })
        : props.primaryButtonText));
    footer = h('div', { className: cx('cds--modal-footer', Array.isArray(sbs) && sbs.length === 2 && 'cds--modal-footer--three-button') }, btns);
  }

  var closeBtn = h(ModalCloseButton, { key: 'close', label: props.closeButtonLabel, onClick: onClose });
  return h(ModalCore, {
    open: props.open, inline: inline, inlineHeight: props.inlineHeight, danger: danger, size: props.size,
    isFullWidth: props.isFullWidth, className: props.className, onRequestClose: onClose, onRequestSubmit: onSubmit,
    onKeyDown: props.onKeyDown, shouldSubmitOnEnter: props.shouldSubmitOnEnter, selectorPrimaryFocus: props.selectorPrimaryFocus,
    launcherButtonRef: props.launcherButtonRef, ariaLabel: ariaLabel,
    labelledBy: props.modalLabel ? labelId : headingId,
    alertdialog: props.alert && !passive, describedBy: bodyId,
    closeOnOutsideClick: (passive && !props.preventCloseOnClickOutside) || (!passive && props.preventCloseOnClickOutside === false)
  },
    h('div', { className: 'cds--modal-header' },
      passive ? closeBtn : null,
      props.modalLabel ? h('h2', { id: labelId, className: 'cds--modal-header__label' }, props.modalLabel) : null,
      h('h2', { id: headingId, className: 'cds--modal-header__heading' }, props.modalHeading),
      passive ? null : closeBtn),
    h(ModalContentArea, { id: bodyId, hasScrollingContent: props.hasScrollingContent, ariaLabel: ariaLabel, labelledBy: props.modalLabel ? labelId : headingId }, props.children),
    footer);
}

// ---------------------------------------------------------------------------
// ComposedModal, ModalHeader, ModalBody, ModalFooter
// ---------------------------------------------------------------------------
function ComposedModal(props) {
  var uid = useId();
  var st = useState(!!props.open); var isOpen = st[0], setOpen = st[1];
  useEffect(function () { setOpen(!!props.open); }, [props.open]);
  var headingId = 'cds-composed-modal-heading-' + uid, labelId = 'cds-composed-modal-label-' + uid;
  function closeModal(evt) {
    if (!props.onClose || props.onClose(evt) !== false) setOpen(false);
  }
  var ctx = { closeModal: closeModal, danger: !!props.danger, headingId: headingId, labelId: labelId };
  var hasFooter = React.Children.toArray(props.children).some(function (c) { return c && c.type === ModalFooter; });
  return h(D1ModalContext.Provider, { value: ctx },
    h(ModalCore, {
      open: isOpen, inline: props.inline || props.static, inlineHeight: props.inlineHeight, danger: props.danger, size: props.size,
      className: props.className, containerClassName: props.containerClassName, onRequestClose: closeModal,
      onKeyDown: props.onKeyDown, selectorPrimaryFocus: props.selectorPrimaryFocus, launcherButtonRef: props.launcherButtonRef,
      ariaLabel: props['aria-label'], labelledBy: headingId,
      closeOnOutsideClick: props.preventCloseOnClickOutside === false || (props.preventCloseOnClickOutside === undefined && !hasFooter)
    }, props.children));
}

function ModalHeader(props) {
  var ctx = React.useContext(D1ModalContext) || {};
  function onClose(e) {
    if (props.buttonOnClick) props.buttonOnClick(e);
    if (props.closeModal) props.closeModal(e); else if (ctx.closeModal) ctx.closeModal(e);
  }
  return h('div', { className: cx('cds--modal-header', props.className) },
    props.label ? h('h2', { id: ctx.labelId, className: cx('cds--modal-header__label', props.labelClassName) }, props.label) : null,
    props.title ? h('h2', { id: ctx.headingId, className: cx('cds--modal-header__heading', props.titleClassName) }, props.title) : null,
    props.children,
    h(ModalCloseButton, { label: props.iconDescription, onClick: onClose }));
}

function ModalBody(props) {
  return h(ModalContentArea, {
    className: props.className, hasForm: props.hasForm, hasScrollingContent: props.hasScrollingContent,
    ariaLabel: props['aria-label']
  }, props.children);
}

function ModalFooter(props) {
  var ctx = React.useContext(D1ModalContext) || {};
  var danger = props.danger != null ? props.danger : ctx.danger;
  var sbs = props.secondaryButtons;
  var three = Array.isArray(sbs) && sbs.length === 2;
  function closeAnd(e) {
    if (props.onRequestClose) props.onRequestClose(e);
    if (props.closeModal) props.closeModal(e); else if (ctx.closeModal) ctx.closeModal(e);
  }
  var kids;
  if (props.children) {
    kids = props.children;
  } else {
    kids = [];
    if (Array.isArray(sbs) && sbs.length > 0 && sbs.length <= 2) {
      sbs.forEach(function (b, i) { kids.push(h(exports.Button, { key: 's' + i, kind: 'secondary', onClick: b.onClick }, b.buttonText)); });
    } else if (props.secondaryButtonText) {
      kids.push(h(exports.Button, { key: 's', kind: 'secondary', onClick: closeAnd }, props.secondaryButtonText));
    }
    if (props.primaryButtonText) {
      kids.push(h(exports.Button, { key: 'p', kind: danger ? 'danger' : 'primary', disabled: props.primaryButtonDisabled, onClick: props.onRequestSubmit || d1Noop }, props.primaryButtonText));
    }
  }
  return h('div', { className: cx('cds--modal-footer', three && 'cds--modal-footer--three-button', props.className) }, kids);
}

exports.Modal = Modal;
exports.ComposedModal = ComposedModal;
exports.ModalHeader = ModalHeader;
exports.ModalBody = ModalBody;
exports.ModalFooter = ModalFooter;

// ---------------------------------------------------------------------------
// Notifications (shared icon mapping, same glyphs as InlineNotification)
// ---------------------------------------------------------------------------
// [glyph, source size]: 32px sources are drawn at 20px by the __icon CSS rule so the outer shape is not cropped.
var D1_NOTE_ICON = { error: ['error--filled', 20], success: ['checkmark--filled', 20], warning: ['warning--filled', 20], 'warning-alt': ['warning--alt--filled', 32], info: ['information--filled', 32], 'info-square': ['information--square--filled', 32] };

function D1NotificationIcon(props) {
  var ic = D1_NOTE_ICON[props.kind];
  if (!ic) return null;
  return h(Icon, { name: ic[0], size: ic[1], className: props.className, title: props.description || (props.kind + ' icon') });
}

function D1NotificationClose(props) {
  return h('button', { type: 'button', className: props.className, 'aria-label': props.label || 'close notification', title: props.label || 'close notification', onClick: props.onClick },
    h(Icon, { name: 'close', size: 16, className: props.iconClassName }));
}

// ToastNotification ---------------------------------------------------------
function ToastNotification(props) {
  var kind = props.kind || 'error';
  var st = useState(true); var isOpen = st[0], setOpen = st[1];
  var timeout = props.timeout || 0;
  var saved = useRef(props.onClose);
  useEffect(function () { saved.current = props.onClose; });
  useEffect(function () {
    if (!timeout) return;
    var id = window.setTimeout(function (ev) {
      setOpen(false);
      if (saved.current) saved.current(ev);
    }, timeout);
    return function () { window.clearTimeout(id); };
  }, [timeout]);
  if (!isOpen) return null;
  function handleClose(evt) {
    if (props.onCloseButtonClick) props.onCloseButtonClick(evt);
    if (!props.onClose || props.onClose(evt) !== false) setOpen(false);
  }
  return h('div', { role: props.role || 'status', className: cx('cds--toast-notification', props.lowContrast && 'cds--toast-notification--low-contrast', 'cds--toast-notification--' + kind, props.className), style: props.style },
    h(D1NotificationIcon, { kind: kind, className: 'cds--toast-notification__icon', description: props.statusIconDescription }),
    h('div', { className: 'cds--toast-notification__details' },
      props.title ? h('div', { className: 'cds--toast-notification__title' }, props.title) : null,
      props.subtitle ? h('div', { className: 'cds--toast-notification__subtitle' }, props.subtitle) : null,
      props.caption ? h('div', { className: 'cds--toast-notification__caption' }, props.caption) : null,
      props.children),
    props.hideCloseButton ? null : h(D1NotificationClose, { className: 'cds--toast-notification__close-button', iconClassName: 'cds--toast-notification__close-icon', label: props['aria-label'], onClick: handleClose }));
}
exports.ToastNotification = ToastNotification;

// ActionableNotification ----------------------------------------------------
function ActionableNotification(props) {
  var kind = props.kind || 'error', inline = !!props.inline;
  var role = props.role || 'alertdialog';
  var st = useState(true); var isOpen = st[0], setOpen = st[1];
  var uid = useId();
  var titleId = 'cds-actionable-' + uid, subId = 'cds-actionable-sub-' + uid;
  var ref = useRef(null);
  var hasFocus = props.hasFocus !== false;
  useEffect(function () {
    if (hasFocus && role === 'alertdialog' && ref.current) {
      var b = ref.current.querySelector('button.cds--actionable-notification__action-button');
      if (b) b.focus();
    }
  }, []);
  if (!isOpen) return null;
  function handleClose(evt) {
    if (props.onCloseButtonClick) props.onCloseButtonClick(evt);
    if (!props.onClose || props.onClose(evt) !== false) setOpen(false);
  }
  function onKeyDown(e) {
    if (props.onKeyDown) props.onKeyDown(e);
    if (e.key === 'Escape' && props.closeOnEscape !== false) { handleClose(e); return; }
    if (e.key === 'Tab' && role === 'alertdialog' && ref.current) {
      var f = d1Focusable(ref.current);
      if (!f.length) return;
      var first = f[0], last = f[f.length - 1];
      if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus(); }
      else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus(); }
    }
  }
  return h('div', {
    ref: ref, role: role, 'aria-labelledby': props.title ? titleId : (props.subtitle ? subId : undefined), onKeyDown: onKeyDown, style: props.style,
    className: cx('cds--actionable-notification', !inline && 'cds--actionable-notification--toast', props.lowContrast && 'cds--actionable-notification--low-contrast', 'cds--actionable-notification--' + kind, props.hideCloseButton && 'cds--actionable-notification--hide-close-button', props.className)
  },
    h('div', { className: 'cds--actionable-notification__details' },
      h(D1NotificationIcon, { kind: kind, className: inline ? 'cds--actionable-notification__icon cds--actionable-notification__icon--inline' : 'cds--actionable-notification__icon', description: props.statusIconDescription }),
      h('div', { className: 'cds--actionable-notification__text-wrapper' },
        h('div', { className: 'cds--actionable-notification__content' },
          props.title ? h('div', { id: titleId, className: 'cds--actionable-notification__title' }, props.title) : null,
          props.subtitle ? h('div', { id: subId, className: 'cds--actionable-notification__subtitle' }, props.subtitle) : null,
          props.caption ? h('div', { className: 'cds--actionable-notification__caption' }, props.caption) : null,
          props.children))),
    h('div', { className: 'cds--actionable-notification__button-wrapper' },
      props.actionButtonLabel ? h(exports.Button, { kind: inline ? 'ghost' : 'tertiary', size: 'sm', className: 'cds--actionable-notification__action-button', onClick: props.onActionButtonClick }, props.actionButtonLabel) : null,
      props.hideCloseButton ? null : h(D1NotificationClose, { className: 'cds--actionable-notification__close-button', iconClassName: 'cds--actionable-notification__close-icon', label: props['aria-label'], onClick: handleClose })));
}
exports.ActionableNotification = ActionableNotification;

})(Carbon,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx);

/* ---- D2 ---- */
(function(exports,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx){
/* @components: Popover, PopoverContent, Tooltip, Toggletip, ToggletipButton, ToggletipContent, ToggletipActions, IconButton, CopyButton, IconIndicator, ShapeIndicator, BadgeIndicator */

// ---------------------------------------------------------------- Popover
var PopoverContext = React.createContext(null);

function Popover(props) {
  var as = props.as || 'span';
  var align = props.align || 'bottom';
  var caret = props.caret === undefined ? true : props.caret;
  var dropShadow = props.dropShadow === undefined ? true : props.dropShadow;
  var open = !!props.open;
  var localRef = useRef(null);
  var onRequestClose = props.onRequestClose;
  var reqRef = useRef(onRequestClose);
  reqRef.current = onRequestClose;

  useEffect(function () {
    if (!open) return undefined;
    function down(e) {
      var el = localRef.current;
      if (el && !el.contains(e.target) && reqRef.current) reqRef.current();
    }
    document.addEventListener('mousedown', down, true);
    return function () { document.removeEventListener('mousedown', down, true); };
  }, [open]);

  function setRef(node) {
    localRef.current = node;
    var r = props.forwardedRef;
    if (typeof r === 'function') r(node); else if (r) r.current = node;
  }
  function onBlur(e) {
    if (props.onBlur) props.onBlur(e);
    if (open && reqRef.current && e.relatedTarget && !e.currentTarget.contains(e.relatedTarget)) reqRef.current();
  }
  var extra = {};
  Object.keys(props).forEach(function (k) {
    if (['as', 'align', 'caret', 'dropShadow', 'highContrast', 'border', 'open', 'className', 'children', 'onRequestClose', 'forwardedRef', 'onBlur'].indexOf(k) < 0) extra[k] = props[k];
  });
  var cls = cx('cds--popover-container', caret && 'cds--popover--caret', dropShadow && 'cds--popover--drop-shadow', props.border && 'cds--popover--border',
    props.highContrast && 'cds--popover--high-contrast', open && 'cds--popover--open', 'cds--popover--' + align, props.className);
  return h(PopoverContext.Provider, { value: { open: open } },
    h(as, Object.assign({}, extra, { className: cls, ref: setRef, onBlur: onBlur }), props.children));
}
Popover.displayName = 'Popover';

var PopoverContent = React.forwardRef(function PopoverContent(props, ref) {
  var rest = {};
  Object.keys(props).forEach(function (k) { if (k !== 'className' && k !== 'children') rest[k] = props[k]; });
  return h('span', Object.assign({}, rest, { className: 'cds--popover' }),
    h('span', { className: cx('cds--popover-content', props.className), ref: ref }, props.children),
    h('span', { className: 'cds--popover-caret' }));
});
PopoverContent.displayName = 'PopoverContent';

exports.Popover = Popover;
exports.PopoverContent = PopoverContent;

// ---------------------------------------------------------------- Tooltip
function Tooltip(props) {
  var align = props.align || 'top';
  var enterDelay = props.enterDelayMs === undefined ? 100 : props.enterDelayMs;
  var leaveDelay = props.leaveDelayMs === undefined ? 300 : props.leaveDelayMs;
  var st = useState(!!props.defaultOpen); var openState = st[0], setOpenState = st[1];
  var timer = useRef(null);
  var mouseFocus = useRef(false);
  var autoId = useId(); var id = props.id || ('tooltip-' + autoId);
  var forced = props.open !== undefined;
  var open = forced ? !!props.open : openState;

  function setOpen(v, delay) {
    clearTimeout(timer.current);
    if (delay) timer.current = setTimeout(function () { setOpenState(v); }, delay);
    else setOpenState(v);
  }
  useEffect(function () { return function () { clearTimeout(timer.current); }; }, []);
  useEffect(function () {
    if (!open) return undefined;
    function key(e) { if (e.key === 'Escape') { setOpen(false); } }
    document.addEventListener('keydown', key);
    return function () { document.removeEventListener('keydown', key); };
  }, [open]);

  var child = React.Children.only(props.children);
  var cp = child.props || {};
  var hasLabel = !!props.label;
  var trig = {
    onFocus: function () { if (!mouseFocus.current) setOpen(true); },
    onBlur: function () { setOpen(false); mouseFocus.current = false; },
    onClick: function () { if (props.closeOnActivation) setOpen(false); },
    onMouseEnter: function () { setOpen(true, enterDelay); },
    onMouseLeave: function () { setOpen(false, leaveDelay); },
    onMouseDown: function () { mouseFocus.current = true; }
  };
  var handlers = {};
  Object.keys(trig).forEach(function (k) {
    handlers[k] = function (e) { trig[k](e); if (cp[k]) cp[k](e); };
  });
  var cloned = React.cloneElement(child, Object.assign({}, handlers, {
    'aria-labelledby': cp['aria-labelledby'] || (hasLabel ? id : undefined),
    'aria-describedby': cp['aria-describedby'] || (!hasLabel ? id : undefined)
  }));
  return h(Popover, {
    align: align, open: open, caret: props.caret,
    dropShadow: props.dropShadow === undefined ? false : props.dropShadow,
    highContrast: props.highContrast === undefined ? true : props.highContrast,
    className: cx('cds--tooltip', props.className),
    onMouseLeave: function () { setOpen(false, leaveDelay); },
    forwardedRef: props.forwardedRef
  },
    h('div', { className: 'cds--tooltip-trigger__wrapper' }, cloned),
    h(PopoverContent, {
      className: 'cds--tooltip-content', id: id, role: 'tooltip', 'aria-hidden': open ? 'false' : 'true',
      onMouseEnter: function () { setOpen(true, enterDelay); }
    }, props.label || props.description));
}
exports.Tooltip = Tooltip;

// ---------------------------------------------------------------- Toggletip
var ToggletipContext = React.createContext(null);

function Toggletip(props) {
  var st = useState(!!props.defaultOpen); var open = st[0], setOpen = st[1];
  var ref = useRef(null);
  var autoId = useId(); var id = 'toggletip-' + autoId;
  useEffect(function () {
    if (!open) return undefined;
    function down(e) { if (ref.current && !ref.current.contains(e.target)) setOpen(false); }
    function blur() { setOpen(false); }
    document.addEventListener('mousedown', down, true);
    window.addEventListener('blur', blur);
    return function () { document.removeEventListener('mousedown', down, true); window.removeEventListener('blur', blur); };
  }, [open]);
  function onKeyDown(e) {
    if (open && e.key === 'Escape') {
      e.stopPropagation(); setOpen(false);
      var b = ref.current && ref.current.querySelector('.cds--toggletip-button');
      if (b) b.focus();
    }
  }
  function onBlur(e) {
    if (open && e.relatedTarget === null) return;
    if (!e.currentTarget.contains(e.relatedTarget)) setOpen(false);
  }
  var value = {
    id: id, open: open,
    toggle: function () { setOpen(!open); }
  };
  return h(ToggletipContext.Provider, { value: value },
    h(Popover, {
      align: props.align || 'bottom', as: props.as, caret: true, dropShadow: false, highContrast: true, open: open,
      className: cx('cds--toggletip', open && 'cds--toggletip--open', props.className),
      onKeyDown: onKeyDown, onBlur: onBlur, forwardedRef: ref
    }, props.children));
}
function ToggletipButton(props) {
  var t = React.useContext(ToggletipContext) || {};
  var kids = props.children || h(Icon, { name: 'information', size: 16 });
  return h('button', {
    type: 'button', className: cx('cds--toggletip-button', props.className), 'aria-label': props.label || 'Show information',
    'aria-expanded': !!t.open, 'aria-controls': t.id, 'aria-describedby': t.open ? t.id : undefined, onClick: t.toggle
  }, kids);
}
function ToggletipContent(props) {
  var t = React.useContext(ToggletipContext) || {};
  return h(PopoverContent, { className: props.className, id: t.id },
    h('div', { className: 'cds--toggletip-content' }, props.children));
}
function ToggletipActions(props) {
  return h('div', { className: cx('cds--toggletip-actions', props.className) }, props.children);
}
exports.Toggletip = Toggletip;
exports.ToggletipButton = ToggletipButton;
exports.ToggletipContent = ToggletipContent;
exports.ToggletipActions = ToggletipActions;

// ---------------------------------------------------------------- BadgeIndicator
function BadgeIndicator(props) {
  var count = props.count;
  var shown = count && count > 999 ? '999+' : count;
  return h('div', { id: props.id, className: cx('cds--badge-indicator', count && 'cds--badge-indicator--count', props.className) }, shown);
}
exports.BadgeIndicator = BadgeIndicator;

// ---------------------------------------------------------------- IconButton
function IconButton(props) {
  var kind = props.kind || 'primary', size = props.size || 'lg';
  var disabled = props.disabled;
  var badgeId = useId();
  var iconEl = props.renderIcon
    ? h(Icon, { name: props.renderIcon, size: 16, className: 'cds--btn__icon' })
    : (React.isValidElement(props.children)
        ? React.cloneElement(props.children, { className: cx(props.children.props && props.children.props.className, 'cds--btn__icon') })
        : props.children);
  var hasBadge = !disabled && props.badgeCount !== undefined;
  // Pass through native/aria/data attributes and extra handlers (Tooltip chains its own handlers with these).
  var IB_OWN = ['label', 'renderIcon', 'children', 'kind', 'size', 'align', 'disabled', 'isSelected', 'badgeCount', 'defaultOpen', 'open', 'closeOnActivation',
    'enterDelayMs', 'leaveDelayMs', 'highContrast', 'dropShadow', 'wrapperClasses', 'className', 'onClick', 'type', 'aria-describedby', 'aria-label'];
  var rest = {};
  Object.keys(props).forEach(function (k) { if (IB_OWN.indexOf(k) < 0) rest[k] = props[k]; });
  var btnProps = Object.assign(rest, {
    type: props.type || 'button', disabled: disabled, onClick: props.onClick,
    className: cx('cds--btn', 'cds--btn--' + kind, 'cds--btn--' + size, 'cds--btn--icon-only', props.isSelected && 'cds--btn--selected', props.className),
    'aria-describedby': props['aria-describedby'] || (hasBadge ? 'badge-' + badgeId : undefined),
    'aria-label': props['aria-label'],
    // A selectable icon button is a toggle: expose its state.
    'aria-pressed': rest['aria-pressed'] !== undefined ? rest['aria-pressed'] : (props.isSelected !== undefined ? !!props.isSelected : undefined)
  });
  var button = h('button', btnProps, iconEl,
    hasBadge ? h(BadgeIndicator, { id: 'badge-' + badgeId, count: props.badgeCount > 0 ? props.badgeCount : undefined }) : null);
  return h(Tooltip, {
    align: props.align, closeOnActivation: props.closeOnActivation === undefined ? true : props.closeOnActivation,
    className: cx(props.wrapperClasses, 'cds--icon-tooltip', disabled && 'cds--icon-tooltip--disabled'),
    defaultOpen: props.defaultOpen, open: props.open, dropShadow: props.dropShadow,
    enterDelayMs: props.enterDelayMs === undefined ? 100 : props.enterDelayMs,
    highContrast: props.highContrast === undefined ? true : props.highContrast,
    label: props.label, leaveDelayMs: props.leaveDelayMs === undefined ? 100 : props.leaveDelayMs
  }, button);
}
exports.IconButton = IconButton;

// ---------------------------------------------------------------- CopyButton
function CopyButton(props) {
  var st = useState(!!props.defaultCopied); var copied = st[0], setCopied = st[1];
  var timeout = props.feedbackTimeout === undefined ? 2000 : props.feedbackTimeout;
  var timer = useRef(null);
  useEffect(function () { return function () { clearTimeout(timer.current); }; }, []);
  function click(e) {
    if (props.onClick) props.onClick(e);
    setCopied(true);
    clearTimeout(timer.current);
    timer.current = setTimeout(function () { setCopied(false); }, timeout);
  }
  var desc = props.iconDescription || 'Copy to clipboard';
  var btn = h(IconButton, {
    align: props.align || 'bottom', kind: 'primary', size: props.size || 'md', className: cx('cds--copy-btn', 'cds--copy', props.className),
    label: copied ? (props.feedback || 'Copied!') : desc, closeOnActivation: false,
    open: copied || props.open ? true : undefined, defaultOpen: props.defaultOpen,
    leaveDelayMs: copied ? timeout : undefined, onClick: click, disabled: props.disabled
  }, h('svg', { className: 'cds--icon', width: 16, height: 16, viewBox: '0 0 32 32', 'aria-hidden': 'true', focusable: 'false' },
    h('path', { d: 'M28 10V28H10V10h18m0-2H10a2 2 0 0 0-2 2V28a2 2 0 0 0 2 2H28a2 2 0 0 0 2-2V10a2 2 0 0 0-2-2zM4 18H2V4A2 2 0 0 1 4 2H18V4H4z' })));
  // Changing the tooltip label is not announced by itself: a polite live region reads the feedback.
  return h(React.Fragment, null, btn,
    h('span', { className: 'cds--visually-hidden', 'aria-live': 'polite' }, copied ? (props.feedback || 'Copied!') : ''));
}
exports.CopyButton = CopyButton;

// ---------------------------------------------------------------- IconIndicator / ShapeIndicator
// Glyphs are drawn here (the icon set shipped with the bundle does not contain these). Sub-paths use evenodd so cut-outs show the surface behind.
var Q = 'M16 21.9a1.6 1.6 0 1 1 0 3.2 1.6 1.6 0 0 1 0-3.2zM14.9 19.6v-2.3c0-.9.6-1.4 1.3-1.8.7-.4 1.4-.8 1.4-1.7 0-1-.7-1.6-1.7-1.6s-1.7.6-1.8 1.7h-2.2c.1-2.1 1.7-3.6 4-3.6 2.2 0 3.9 1.4 3.9 3.4 0 1.5-.9 2.3-1.8 2.9-.7.5-1.1.8-1.1 1.4v1.6h-2z';
var ICON_GLYPHS = {
  'failed': ['0 0 16 16', ['M8 1a7 7 0 1 0 0 14A7 7 0 0 0 8 1zM5.2 4.5 8 7.3l2.8-2.8.7.7L8.7 8l2.8 2.8-.7.7L8 8.7l-2.8 2.8-.7-.7L7.3 8 4.5 5.2z']],
  'caution-major': ['0 0 16 16', ['M8 15 .5 1.5h15L8 15zM7.4 4h1.2v4.6H7.4zM8 11.9a.7.7 0 1 0 0-1.4.7.7 0 0 0 0 1.4z']],
  'caution-minor': ['0 0 16 16', ['M8 1 .5 14.5h15L8 1zM7.4 5.4h1.2V10H7.4zM8 13.3a.7.7 0 1 0 0-1.4.7.7 0 0 0 0 1.4z']],
  'undefined': ['0 0 32 32', ['M16 1 31 16 16 31 1 16zM' + Q.slice(1)]],
  'succeeded': ['0 0 16 16', ['M8 1a7 7 0 1 0 0 14A7 7 0 0 0 8 1zM7 11 4.3 8.3l.9-.8L7 9.3l4-3.9.9.8L7 11z']],
  'normal': ['0 0 32 32', ['M14 21.414 9 16.413 10.413 15 14 18.586 21.585 11 23 12.415z', 'M16 2a14 14 0 1 0 14 14A14 14 0 0 0 16 2zm0 26a12 12 0 1 1 12-12 12 12 0 0 1-12 12z']],
  'in-progress': ['0 0 32 32', ['M16 2a14 14 0 1 0 14 14A14.0158 14.0158 0 0 0 16 2zm0 26a12 12 0 0 1 0-24v12l8.4812 8.4814A11.9625 11.9625 0 0 1 16 28z']],
  'incomplete': ['0 0 32 32', ['M23.7642 6.8593l1.2851-1.5315A13.976 13.976 0 0 0 20.8672 2.887l-.6836 1.8776a11.9729 11.9729 0 0 1 3.5806 2.0947z', 'M27.81 14l1.9677-.4128A13.8888 13.8888 0 0 0 28.14 9.0457L26.4087 10A12.52 12.52 0 0 1 27.81 14z', 'M20.1836 27.2354l.6836 1.8776a13.976 13.976 0 0 0 4.1821-2.4408l-1.2851-1.5315a11.9729 11.9729 0 0 1-3.5806 2.0947z', 'M26.4087 22 28.14 23a14.14 14.14 0 0 0 1.6382-4.5872L27.81 18.0659A12.1519 12.1519 0 0 1 26.4087 22z', 'M16 30V2a14 14 0 0 0 0 28z']],
  'not-started': ['0 0 32 32', ['M7.7 4.7a14.7 14.7 0 0 0-3 3.1L6.3 9A13.26 13.26 0 0 1 8.9 6.3z', 'M4.6 12.3l-1.9-.6A12.51 12.51 0 0 0 2 16h2a11.48 11.48 0 0 1 .6-3.7z', 'M2.7 20.4a14.4 14.4 0 0 0 2 3.9l1.6-1.2a12.89 12.89 0 0 1-1.7-3.3z', 'M7.8 27.3a14.4 14.4 0 0 0 3.9 2l.6-1.9A12.89 12.89 0 0 1 9 25.7z', 'M11.7 2.7l.6 1.9A11.48 11.48 0 0 1 16 4V2a12.51 12.51 0 0 0-4.3.7z', 'M24.2 27.3a15.18 15.18 0 0 0 3.1-3.1L25.7 23A11.53 11.53 0 0 1 23 25.7z', 'M27.4 19.7l1.9.6A15.47 15.47 0 0 0 30 16h-2a11.48 11.48 0 0 1-.6 3.7z', 'M29.2 11.6a14.4 14.4 0 0 0-2-3.9L25.6 8.9a12.89 12.89 0 0 1 1.7 3.3z', 'M24.1 4.6a14.4 14.4 0 0 0-3.9-2l-.6 1.9a12.89 12.89 0 0 1 3.3 1.7z', 'M20.3 29.3l-.6-1.9A11.48 11.48 0 0 1 16 28v2a21.42 21.42 0 0 0 4.3-.7z']],
  'pending': ['0 0 32 32', ['M16 2a14 14 0 1 0 14 14A14 14 0 0 0 16 2zM8 18a2 2 0 1 1 2-2 2 2 0 0 1-2 2zm8 0a2 2 0 1 1 2-2 2 2 0 0 1-2 2zm8 0a2 2 0 1 1 2-2 2 2 0 0 1-2 2z']],
  'unknown': ['0 0 32 32', ['M16 2a14 14 0 1 0 14 14A14 14 0 0 0 16 2zM' + Q.slice(1)]],
  'informative': ['0 0 32 32', ['M26 4H6a2 2 0 0 0-2 2v20a2 2 0 0 0 2 2h20a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2zM14.875 8h2.25v11h-2.25zM16 25.5a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3z']]
};
var SHAPE_GLYPHS = {
  'failed': ['M5.2 1h5.6L15 5.2v5.6L10.8 15H5.2L1 10.8V5.2L5.2 1zM4.7 5.4 7.3 8l-2.6 2.6.7.7L8 8.7l2.6 2.6.7-.7L8.7 8l2.6-2.6-.7-.7L8 7.3 5.4 4.7z'],
  'critical': ['M4.5 1.5h7L15.5 8l-4 6.5h-7L.5 8z'],
  'high': ['M8 1 15.2 14H.8z'],
  'medium': ['M8 .5 15.5 8 8 15.5.5 8z', 'M8 2.6 13.4 8 8 13.4 2.6 8z'],
  'low': ['M8 15 .8 2h14.4z', 'M8 15 .8 2h14.4zM8 11.8 3.1 3.2h9.8z'],
  'cautious': ['M8 1 15.2 14H.8z', 'M8 4.2 12.9 12.9H3.1z'],
  'undefined': ['M8 .5 15.5 8 8 15.5.5 8z'],
  'stable': ['M8 2a6 6 0 1 0 0 12A6 6 0 0 0 8 2z'],
  'informative': ['M8 15 .8 2h14.4z'],
  'incomplete': ['M8 2a6 6 0 1 0 0 12A6 6 0 0 0 8 2zM8 4a4 4 0 0 1 4 4H4a4 4 0 0 1 4-4z'],
  'draft': ['M8 2a6 6 0 1 0 0 12A6 6 0 0 0 8 2zm0 2a4 4 0 1 1 0 8 4 4 0 0 1 0-8z']
};
function glyph(vb, paths, cls, size) {
  return h('svg', { className: cls, width: size, height: size, viewBox: vb, 'aria-hidden': 'true', focusable: 'false', fillRule: 'evenodd' },
    paths.map(function (d, i) { return h('path', { key: i, d: d, fillRule: 'evenodd' }); }));
}
function IconIndicator(props) {
  var g = ICON_GLYPHS[props.kind]; if (!g) return null;
  var size = props.size === 20 ? 20 : 16;
  return h('div', { className: cx('cds--icon-indicator', size === 20 && 'cds--icon-indicator--20', props.className) },
    glyph(g[0], g[1], 'cds--icon-indicator--' + props.kind, size), props.label);
}
function ShapeIndicator(props) {
  var g = SHAPE_GLYPHS[props.kind]; if (!g) return null;
  return h('div', { className: cx('cds--shape-indicator', props.textSize === 14 && 'cds--shape-indicator--14', props.className) },
    glyph('0 0 16 16', g, 'cds--shape-indicator--' + props.kind, 16), props.label);
}
exports.IconIndicator = IconIndicator;
exports.ShapeIndicator = ShapeIndicator;

})(Carbon,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx);

/* ---- E ---- */
(function(exports,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx){
/* @components: Layer, LayerLevel, Stack, Grid, Column, FlexGrid, Row, ButtonSet, AspectRatio, SelectableTile, ExpandableTile, TileAboveTheFoldContent, TileBelowTheFoldContent, RadioTile, TileGroup, ListItem, OrderedList, UnorderedList */
function mergeStyle(a, b) { return Object.assign({}, a || {}, b || {}); }

// Layer ---------------------------------------------------------------------
// LayerLevel names the three layers; a nested Layer steps one level up from its context (1 -> 2 -> 3, then stays at 3).
var LayerLevel = { one: 1, two: 2, three: 3 };
var LAYER_NAMES = ['one', 'two', 'three'];
var LayerContext = React.createContext(1);
function Layer(props) {
  var ctx = React.useContext(LayerContext);
  var as = props.as, level = props.level, withBackground = props.withBackground, className = props.className, children = props.children;
  var rest = {}; for (var k in props) { if (k !== 'as' && k !== 'level' && k !== 'withBackground' && k !== 'className' && k !== 'children') rest[k] = props[k]; }
  var lv = level != null ? Math.min(3, Math.max(1, level)) : Math.min(3, ctx + 1);
  var cls = cx('cds--layer-' + LAYER_NAMES[lv - 1], withBackground && 'cds--layer__with-background', className);
  var Base = as || 'div';
  return h(LayerContext.Provider, { value: lv }, h(Base, Object.assign({}, rest, { className: cls }), children));
}
exports.Layer = Layer; exports.LayerLevel = LayerLevel;
// useLayer(): the level (1-3) of the nearest enclosing Layer, 1 outside any Layer (or inside a Theme zone).
function useLayer() { return { level: React.useContext(LayerContext) }; }
exports.useLayer = useLayer;

// Stack ---------------------------------------------------------------------
function Stack(props) {
  var Base = props.as || 'div', orientation = props.orientation || 'vertical', gap = props.gap;
  var rest = {}; for (var k in props) { if (k !== 'as' && k !== 'orientation' && k !== 'gap' && k !== 'className' && k !== 'children' && k !== 'style') rest[k] = props[k]; }
  var style = mergeStyle(props.style);
  if (typeof gap === 'string') style['--cds-stack-gap'] = gap;
  var cls = cx(props.className, 'cds--stack-' + orientation, typeof gap === 'number' && 'cds--stack-scale-' + gap);
  return h(Base, Object.assign({}, rest, { className: cls, style: style }), props.children);
}
exports.Stack = Stack;

// Grid ----------------------------------------------------------------------
var GridContext = React.createContext({ mode: 'flexbox', subgrid: false });
function omit(props, names) { var o = {}; for (var k in props) { if (names.indexOf(k) < 0) o[k] = props[k]; } return o; }
function Grid(props) {
  var g = React.useContext(GridContext);
  var Base = props.as || 'div';
  var mode = props.narrow ? 'narrow' : props.condensed ? 'condensed' : 'wide';
  var rest = omit(props, ['as', 'align', 'condensed', 'narrow', 'fullWidth', 'className', 'children']);
  if (g.subgrid && g.mode === 'css-grid') {
    return h(GridContext.Provider, { value: { mode: 'css-grid', subgrid: true } },
      h(Base, Object.assign({}, rest, { className: cx(props.className, 'cds--subgrid', 'cds--subgrid--' + mode) }), props.children));
  }
  var cls = cx(props.className, 'cds--css-grid', mode === 'condensed' && 'cds--css-grid--condensed', mode === 'narrow' && 'cds--css-grid--narrow',
    props.fullWidth && 'cds--css-grid--full-width', props.align === 'start' && 'cds--css-grid--start', props.align === 'end' && 'cds--css-grid--end');
  return h(GridContext.Provider, { value: { mode: 'css-grid', subgrid: true } }, h(Base, Object.assign({}, rest, { className: cls }), props.children));
}
function FlexGrid(props) {
  var Base = props.as || 'div';
  var rest = omit(props, ['as', 'condensed', 'narrow', 'fullWidth', 'className', 'children']);
  var cls = cx(props.className, 'cds--grid', props.condensed && 'cds--grid--condensed', props.narrow && 'cds--grid--narrow', props.fullWidth && 'cds--grid--full-width');
  return h(GridContext.Provider, { value: { mode: 'flexbox', subgrid: false } }, h(Base, Object.assign({}, rest, { className: cls }), props.children));
}
function Row(props) {
  var Base = props.as || 'div';
  var rest = omit(props, ['as', 'condensed', 'narrow', 'className', 'children']);
  return h(Base, Object.assign({}, rest, { className: cx(props.className, 'cds--row', props.condensed && 'cds--row--condensed', props.narrow && 'cds--row--narrow') }), props.children);
}
var BP_NAMES = ['sm', 'md', 'lg', 'xlg', 'max'];
function cssGridBreakpointClasses(bps) {
  var out = [];
  for (var i = 0; i < bps.length; i++) {
    var bp = bps[i]; if (bp === undefined || bp === null) continue; var n = BP_NAMES[i];
    if (bp === true) { out.push('cds--' + n + ':col-span-auto'); continue; }
    if (typeof bp === 'string') { out.push('cds--' + n + ':col-span-' + bp.replace('%', '')); continue; }
    if (typeof bp === 'number') { out.push('cds--' + n + ':col-span-' + bp); continue; }
    if (typeof bp === 'object') {
      if (typeof bp.offset === 'number') out.push('cds--' + n + ':col-start-' + (bp.offset > 0 ? bp.offset + 1 : 'auto'));
      if (typeof bp.start === 'number') out.push('cds--' + n + ':col-start-' + (bp.start ? bp.start : 'auto'));
      if (typeof bp.end === 'number') out.push('cds--' + n + ':col-end-' + bp.end);
      if (typeof bp.span === 'number') out.push('cds--' + n + ':col-span-' + bp.span);
      else if (typeof bp.span === 'string') out.push('cds--' + n + ':col-span-' + bp.span.slice(0, -1));
    }
  }
  return out;
}
function flexBreakpointClasses(bps) {
  var out = [];
  for (var i = 0; i < bps.length; i++) {
    var bp = bps[i]; if (bp === undefined || bp === null) continue; var n = BP_NAMES[i];
    if (bp === true) { out.push('cds--col-' + n); continue; }
    if (typeof bp === 'number') { out.push('cds--col-' + n + '-' + bp); continue; }
    if (typeof bp === 'object') {
      if (typeof bp.span === 'number') out.push('cds--col-' + n + '-' + bp.span);
      if (bp.span === true) out.push('cds--col-' + n);
      if (typeof bp.offset === 'number') out.push('cds--offset-' + n + '-' + bp.offset);
    }
  }
  return out;
}
function spanClasses(v) {
  if (typeof v === 'number') return ['cds--col-span-' + v];
  if (typeof v === 'string') return ['cds--col-span-' + v.slice(0, -1)];
  var out = [];
  if (v && typeof v === 'object') {
    if (v.span != null) out.push('cds--col-span-' + (typeof v.span === 'string' ? v.span.slice(0, -1) : v.span));
    if (v.start != null) out.push('cds--col-start-' + v.start);
    if (v.end != null) out.push('cds--col-end-' + v.end);
  }
  return out;
}
function Column(props) {
  var g = React.useContext(GridContext);
  var Base = props.as || 'div';
  var rest = omit(props, ['as', 'className', 'children', 'sm', 'md', 'lg', 'xlg', 'max', 'span']);
  var bps = [props.sm, props.md, props.lg, props.xlg, props.max];
  var cls;
  if (g.mode === 'css-grid') {
    cls = cx.apply(null, [props.className].concat(cssGridBreakpointClasses(bps), spanClasses(props.span), ['cds--css-grid-column']));
  } else {
    var fc = flexBreakpointClasses(bps);
    cls = cx.apply(null, [props.className].concat(fc, [fc.length === 0 && 'cds--col']));
  }
  return h(Base, Object.assign({}, rest, { className: cls }), props.children);
}
exports.Grid = Grid; exports.FlexGrid = FlexGrid; exports.Row = Row; exports.Column = Column;

// ButtonSet -----------------------------------------------------------------
var BTN_ORDER = { ghost: 1, 'danger--ghost': 2, tertiary: 3, danger: 5, primary: 6 };
function btnKind(el) { return (React.isValidElement(el) && el.props && el.props.kind) || (React.isValidElement(el) ? 'primary' : 'primary'); }
function btnOrder(kind) { return BTN_ORDER[kind] || 4; }
function ButtonSet(props) {
  var fluid = props.fluid, stacked = props.stacked;
  var innerRef = useRef(null);
  var st = useState(!!stacked); var isStacked = st[0], setIsStacked = st[1];
  var rest = omit(props, ['fluid', 'stacked', 'className', 'children']);
  useLayoutEffect(function () {
    function check() {
      var s = !!stacked;
      if (innerRef.current) { s = window.getComputedStyle(innerRef.current).getPropertyValue('--flex-direction').trim() === 'column'; }
      setIsStacked(s);
    }
    check();
    if (!innerRef.current || typeof ResizeObserver === 'undefined') return;
    var ro = new ResizeObserver(check); ro.observe(innerRef.current);
    return function () { ro.disconnect(); };
  }, [stacked, fluid]);
  var kids = React.Children.toArray(props.children);
  if (fluid) {
    kids = kids.slice().sort(function (a, b) { return (btnOrder(btnKind(a)) - btnOrder(btnKind(b))) * (isStacked ? -1 : 1); });
  }
  var cls = cx(props.className, 'cds--btn-set', (fluid ? isStacked : stacked) && 'cds--btn-set--stacked', fluid && 'cds--btn-set--fluid');
  return h('div', Object.assign({}, rest, { className: cls }),
    fluid ? h('div', { ref: innerRef, className: 'cds--btn-set__fluid-inner cds--btn-set__fluid-inner--auto-stack' }, kids) : props.children);
}
exports.ButtonSet = ButtonSet;

// AspectRatio ---------------------------------------------------------------
function AspectRatio(props) {
  var Base = props.as || 'div', ratio = props.ratio || '1x1';
  var rest = omit(props, ['as', 'ratio', 'className', 'children']);
  return h(Base, Object.assign({}, rest, { className: cx(props.className, 'cds--aspect-ratio', 'cds--aspect-ratio--' + ratio) }), props.children);
}
exports.AspectRatio = AspectRatio;

// Tiles ---------------------------------------------------------------------
function CheckboxGlyph(props) {
  return h('svg', { width: 16, height: 16, viewBox: '0 0 16 16', focusable: 'false', 'aria-hidden': 'true' },
    props.checked
      ? h('path', { fillRule: 'evenodd', d: 'M13 3H3a1 1 0 0 0-1 1v8a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1V4a1 1 0 0 0-1-1ZM7 10.5 4.5 8l.7-.7L7 9.1l3.8-3.8.7.7Z' })
      : h('path', { d: 'M13 4v8H3V4h10m0-1H3a1 1 0 0 0-1 1v8a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1V4a1 1 0 0 0-1-1Z' }));
}
function composeHandlers(fns) { return function (e) { for (var i = 0; i < fns.length; i++) { if (fns[i]) fns[i](e); if (e && e.defaultPrevented) return; } }; }
function isSpaceOrEnter(e) { return e.key === 'Enter' || e.key === ' ' || e.key === 'Spacebar'; }

function SelectableTile(props) {
  var autoId = useId(); var id = props.id || ('st-' + autoId);
  var st = useState(!!props.selected); var sel = st[0], setSel = st[1];
  useEffect(function () { setSel(!!props.selected); }, [props.selected]);
  var disabled = props.disabled;
  var rest = omit(props, ['children', 'className', 'decorator', 'disabled', 'id', 'light', 'name', 'onChange', 'onClick', 'onKeyDown', 'selected', 'tabIndex', 'title', 'value', 'hasRoundedCorners']);
  var decoRef = useRef(null);
  // Clicks and keys inside the decorator (an AILabel button and its callout) belong to it, not to the tile.
  function inDecorator(e) { return !!(decoRef.current && e.target && decoRef.current.contains(e.target)); }
  function change(e, next) { setSel(next); if (props.onChange) props.onChange(e, next, id); }
  function onClick(e) { if (inDecorator(e)) return; e.preventDefault(); change(e, !sel); if (props.onClick) props.onClick(e); }
  function onKeyDown(e) { if (inDecorator(e)) return; if (isSpaceOrEnter(e)) { e.preventDefault(); change(e, !sel); } if (props.onKeyDown) props.onKeyDown(e); }
  var cls = cx('cds--tile', 'cds--tile--selectable', sel && 'cds--tile--is-selected', props.light && 'cds--tile--light', disabled && 'cds--tile--disabled', props.decorator && 'cds--tile--decorator', props.className);
  return h('div', Object.assign({ className: cls, onClick: disabled ? undefined : onClick, onKeyDown: disabled ? undefined : onKeyDown, role: 'checkbox', 'aria-checked': sel, 'aria-disabled': disabled || undefined,
    tabIndex: disabled ? undefined : (props.tabIndex != null ? props.tabIndex : 0), id: id, title: props.title }, rest),
    h('span', { className: 'cds--tile__checkmark cds--tile__checkmark--persistent' }, h(CheckboxGlyph, { checked: sel })),
    h('label', { htmlFor: id, className: 'cds--tile-content' }, props.children),
    props.decorator ? h('div', { ref: decoRef, className: 'cds--tile--inner-decorator' }, props.decorator) : null);
}
exports.SelectableTile = SelectableTile;

function TileAboveTheFoldContent(props) { return h('div', { className: 'cds--tile-content__above-the-fold' }, props.children); }
function TileBelowTheFoldContent(props) { return h('div', { className: 'cds--tile-content__below-the-fold' }, props.children); }
exports.TileAboveTheFoldContent = TileAboveTheFoldContent; exports.TileBelowTheFoldContent = TileBelowTheFoldContent;

var INTERACTIVE_SEL = 'a[href], button, input, select, textarea, [tabindex], [role="button"], [role="link"]';
function ExpandableTile(props) {
  var autoId = useId();
  var st = useState(!!props.expanded); var expanded = st[0], setExpanded = st[1];
  var prev = useRef(!!props.expanded);
  if (!!props.expanded !== prev.current) { prev.current = !!props.expanded; setExpanded(!!props.expanded); }
  var ist = useState(true); var interactive = ist[0], setInteractive = ist[1];
  var tileRef = useRef(null), aboveRef = useRef(null), belowRef = useRef(null), chevRef = useRef(null);
  var kids = React.Children.toArray(props.children);
  var collapsedIcon = props.tileCollapsedIconText || 'Interact to expand Tile';
  var expandedIcon = props.tileExpandedIconText || 'Interact to collapse Tile';
  function apply() {
    var t = tileRef.current, a = aboveRef.current; if (!t || !a) return;
    if (expanded) { t.style.maxHeight = ''; return; }
    var cs = window.getComputedStyle(t);
    var pad = (parseFloat(cs.paddingTop) || 0) + (parseFloat(cs.paddingBottom) || 0);
    t.style.maxHeight = (a.getBoundingClientRect().height + pad) + 'px';
  }
  useLayoutEffect(function () {
    if (!aboveRef.current || !belowRef.current) return;
    function hasInteractive(root) { var els = root.querySelectorAll(INTERACTIVE_SEL); for (var i = 0; i < els.length; i++) { if (els[i] !== chevRef.current) return true; } return false; }
    if (!hasInteractive(aboveRef.current) && !hasInteractive(belowRef.current) && !props.decorator) setInteractive(false);
  }, [props.decorator]);
  useLayoutEffect(apply);
  useEffect(function () {
    if (!aboveRef.current || typeof ResizeObserver === 'undefined') return;
    var ro = new ResizeObserver(apply); ro.observe(aboveRef.current);
    return function () { ro.disconnect(); };
  }, [interactive, expanded]);
  var rest = omit(props, ['children', 'className', 'decorator', 'expanded', 'light', 'onClick', 'onKeyUp', 'tabIndex', 'tileCollapsedIconText', 'tileExpandedIconText', 'tileCollapsedLabel', 'tileExpandedLabel', 'tileMaxHeight', 'tilePadding', 'hasRoundedCorners']);
  function toggle() { setExpanded(!expanded); }
  var base = cx('cds--tile', 'cds--tile--expandable', expanded && 'cds--tile--is-expanded', props.light && 'cds--tile--light', props.decorator && 'cds--tile--decorator', props.className);
  var belowId = 'expandable-tile-' + autoId;
  if (interactive) {
    return h('div', Object.assign({ ref: tileRef, className: cx(base, 'cds--tile--expandable--interactive') }, rest),
      h('div', null,
        props.decorator ? h('div', { className: 'cds--tile--inner-decorator' }, props.decorator) : null,
        h('div', { ref: aboveRef, className: 'cds--tile-content' }, kids[0]),
        h('button', { type: 'button', ref: chevRef, className: 'cds--tile__chevron cds--tile__chevron--interactive', 'aria-expanded': expanded, 'aria-controls': belowId,
          'aria-label': expanded ? expandedIcon : collapsedIcon, onClick: composeHandlers([props.onClick, toggle]), onKeyUp: props.onKeyUp },
          h(Icon, { name: 'chevron--down', size: 16 })),
        h('div', { ref: belowRef, className: 'cds--tile-content', id: belowId }, kids[1])));
  }
  return h('button', Object.assign({ type: 'button', ref: tileRef, className: base, 'aria-expanded': expanded, title: expanded ? expandedIcon : collapsedIcon }, rest,
    { onClick: composeHandlers([props.onClick, toggle]), onKeyUp: props.onKeyUp, tabIndex: props.tabIndex != null ? props.tabIndex : 0 }),
    h('div', null,
      h('div', { ref: aboveRef, className: 'cds--tile-content' }, kids[0]),
      h('div', { className: 'cds--tile__chevron' },
        h('span', null, expanded ? props.tileExpandedLabel : props.tileCollapsedLabel),
        h(Icon, { name: 'chevron--down', size: 16 })),
      h('div', { ref: belowRef, className: 'cds--tile-content' }, kids[1])));
}
exports.ExpandableTile = ExpandableTile;

function RadioTile(props) {
  var autoId = useId(); var id = props.id || ('rt-' + autoId);
  var disabled = props.disabled, checked = props.checked, value = props.value, name = props.name;
  var onChange = props.onChange || function () {};
  var labelProps = omit(props, ['children', 'className', 'decorator', 'disabled', 'light', 'checked', 'name', 'value', 'id', 'onChange', 'tabIndex', 'hasRoundedCorners', 'required', 'aria-describedby', 'aria-labelledby']);
  var cls = cx(props.className, 'cds--tile', 'cds--tile--selectable', 'cds--tile--radio', checked && 'cds--tile--is-selected', props.light && 'cds--tile--light', disabled && 'cds--tile--disabled', props.decorator && 'cds--tile--decorator');
  function onKeyDown(e) { if (isSpaceOrEnter(e)) { e.preventDefault(); onChange(value, name, e); } }
  var inputProps = { type: 'radio', className: 'cds--tile-input', id: id, name: name, value: value, checked: !!checked, disabled: disabled, required: props.required,
    tabIndex: disabled ? undefined : (props.tabIndex != null ? props.tabIndex : 0),
    onChange: disabled ? undefined : function (e) { onChange(value, name, e); }, onKeyDown: disabled ? undefined : onKeyDown };
  if (props['aria-describedby']) inputProps['aria-describedby'] = props['aria-describedby'];
  if (props['aria-labelledby']) inputProps['aria-labelledby'] = props['aria-labelledby'];
  return h('div', null,
    h('input', inputProps),
    h('label', Object.assign({}, labelProps, { htmlFor: id, className: cls }),
      h('span', { className: 'cds--tile__checkmark' }, h(Icon, { name: 'checkmark--filled', size: 16 })),
      h('span', { className: 'cds--tile-content' }, props.children),
      props.decorator ? h('div', { className: 'cds--tile--inner-decorator' }, props.decorator) : null));
}
exports.RadioTile = RadioTile;

function TileGroup(props) {
  var st = useState(props.valueSelected !== undefined ? props.valueSelected : props.defaultSelected); var selected = st[0], setSelected = st[1];
  useEffect(function () { if (props.valueSelected !== undefined && props.valueSelected !== selected) setSelected(props.valueSelected); }, [props.valueSelected]);
  function handle(value, name, evt) { if (value !== selected) { setSelected(value); if (props.onChange) props.onChange(value, name || '', evt); } }
  function walk(nodes) {
    return React.Children.map(nodes, function (child) {
      if (!React.isValidElement(child)) return child;
      if (child.type === exports.RadioTile) {
        var p = child.props;
        return React.cloneElement(child, { required: props.required, name: props.name, key: p.value, onChange: handle, checked: p.value === selected, disabled: props.disabled || p.disabled });
      }
      var inner = child.props && child.props.children;
      if (inner && React.Children.count(inner) > 0) return React.cloneElement(child, undefined, walk(inner));
      return child;
    });
  }
  return h('fieldset', { className: props.className || 'cds--tile-group', disabled: props.disabled },
    props.legend ? h('legend', { className: 'cds--label' }, props.legend) : null,
    h('div', null, walk(props.children)));
}
exports.TileGroup = TileGroup;

// Lists ---------------------------------------------------------------------
function ListItem(props) {
  return h('li', Object.assign({}, omit(props, ['className', 'children']), { className: cx('cds--list__item', props.className) }), props.children);
}
function OrderedList(props) {
  var cls = cx(props.native ? 'cds--list--ordered--native' : 'cds--list--ordered', props.nested && 'cds--list--nested', props.isExpressive && 'cds--list--expressive', props.className);
  return h('ol', Object.assign({}, omit(props, ['className', 'nested', 'native', 'isExpressive']), { className: cls }));
}
function UnorderedList(props) {
  var cls = cx('cds--list--unordered', props.className, props.nested && 'cds--list--nested', props.isExpressive && 'cds--list--expressive');
  return h('ul', Object.assign({}, omit(props, ['className', 'nested', 'isExpressive']), { className: cls }));
}
exports.ListItem = ListItem; exports.OrderedList = OrderedList; exports.UnorderedList = UnorderedList;

})(Carbon,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx);

/* ---- G1 ---- */
(function(exports,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx){
/* @components: AILabel, AILabelContent, AILabelActions, AISkeletonText, AISkeletonPlaceholder, AISkeletonIcon, ChatButton */

// ---------------------------------------------------------------- AILabel
// A minimal Toggletip of our own (so the trigger can carry the AI label's own
// aria-label and a controlled/uncontrolled `open`), built on exports.Popover.
var AILabelContext = React.createContext(null);

function AILabelActions(props) {
  return h('div', { className: cx('cds--toggletip-actions', 'cds--ai-label-actions', props.className) }, props.children);
}
AILabelActions.displayName = 'AILabelActions';

function AILabelContent(props) {
  var t = React.useContext(AILabelContext) || {};
  var PopoverContent = exports.PopoverContent;
  var withActions = React.Children.toArray(props.children).some(function (c) { return c && c.type === AILabelActions; });
  return h(PopoverContent, { className: cx(props.className, 'cds--ai-label-content', withActions && 'cds--ai-label-content--with-actions'), id: t.id },
    h('div', { className: 'cds--toggletip-content' }, props.children));
}
AILabelContent.displayName = 'AILabelContent';

var AILabel = React.forwardRef(function AILabel(props, ref) {
  var aiText = props.aiText || 'AI';
  var kind = props.kind || 'default';
  var size = props.size || 'xs';
  var textLabel = props.textLabel || props.aiTextLabel;
  var ariaLabel = props['aria-label'] || props.slugLabel || 'Show information';
  var revertLabel = props.revertLabel || 'Revert to AI input';
  var autoId = useId();
  var id = 'AILabel-' + autoId;
  var tipId = 'ai-label-tip-' + autoId;

  var controlled = props.open !== undefined;
  var st = useState(!!props.defaultOpen);
  var open = controlled ? !!props.open : st[0];
  var wrapRef = useRef(null);
  function setOpen(v) {
    if (!controlled) st[1](v);
    if (props.onOpenChange) props.onOpenChange(v);
  }
  var openRef = useRef(open); openRef.current = open;
  useEffect(function () {
    if (!open) return undefined;
    function down(e) { if (wrapRef.current && !wrapRef.current.contains(e.target)) setOpen(false); }
    function blur() { setOpen(false); }
    document.addEventListener('mousedown', down, true);
    window.addEventListener('blur', blur);
    return function () { document.removeEventListener('mousedown', down, true); window.removeEventListener('blur', blur); };
  }, [open]);

  function setRefs(node) {
    wrapRef.current = node;
    if (typeof ref === 'function') ref(node); else if (ref) ref.current = node;
  }

  var rootCls = cx(props.className, 'cds--ai-label', props.revertActive && 'cds--ai-label--revert');

  if (props.revertActive) {
    var IconButton = exports.IconButton;
    return h('div', { className: rootCls, ref: setRefs, id: id },
      h(IconButton, { kind: 'ghost', size: 'sm', label: revertLabel, 'aria-label': revertLabel, align: props.align, onClick: function (e) { if (props.onRevertClick) props.onRevertClick(e); } },
        h(Icon, { name: 'undo', size: 16 })));
  }

  var Popover = exports.Popover;
  var ariaText = textLabel ? aiText + ' ' + textLabel : aiText + ' ' + ariaLabel;
  var btnCls = cx('cds--toggletip-button', 'cds--ai-label__button', 'cds--ai-label__button--' + size, 'cds--ai-label__button--' + kind,
    kind === 'inline' && textLabel && 'cds--ai-label__button--inline-with-content');
  var hasContent = React.Children.toArray(props.children).length > 0;

  function onKeyDown(e) {
    if (open && e.key === 'Escape') {
      e.stopPropagation(); setOpen(false);
      var b = wrapRef.current && wrapRef.current.querySelector('.cds--ai-label__button');
      if (b) b.focus();
    }
  }
  function onBlur(e) {
    if (open && e.relatedTarget === null) return;
    if (!e.currentTarget.contains(e.relatedTarget)) setOpen(false);
  }

  return h('div', { className: rootCls, ref: setRefs, id: id },
    h(AILabelContext.Provider, { value: { id: tipId, open: open } },
      h(Popover, {
        align: props.align || 'bottom', caret: true, dropShadow: false, highContrast: true, open: open && hasContent,
        className: cx('cds--toggletip', open && 'cds--toggletip--open'), onKeyDown: onKeyDown, onBlur: onBlur
      },
        h('button', {
          type: 'button', className: btnCls,
          'aria-label': kind === 'inline' && textLabel ? undefined : ariaText,
          'aria-expanded': open, 'aria-controls': tipId,
          onClick: function () { setOpen(!open); }
        },
          h('span', { className: 'cds--ai-label__text' }, aiText),
          kind === 'inline' && textLabel ? h('span', { className: 'cds--ai-label__additional-text' }, textLabel) : null),
        props.children)));
});
AILabel.displayName = 'AILabel';

exports.AILabel = AILabel;
exports.AILabelContent = AILabelContent;
exports.AILabelActions = AILabelActions;

// ---------------------------------------------------------------- AISkeleton
function AISkeletonText(props) {
  var SkeletonText = exports.SkeletonText;
  return h(SkeletonText, {
    className: cx(props.className, 'cds--skeleton__text--ai'),
    heading: props.heading, lineCount: props.lineCount, paragraph: props.paragraph, width: props.width
  });
}
function AISkeletonPlaceholder(props) {
  var SkeletonPlaceholder = exports.SkeletonPlaceholder;
  return h(SkeletonPlaceholder, { className: cx(props.className, 'cds--skeleton__placeholder--ai'), style: props.style });
}
function AISkeletonIcon(props) {
  return h('div', { className: cx('cds--icon--skeleton', 'cds--skeleton__icon--ai', props.className), style: props.style });
}
exports.AISkeletonText = AISkeletonText;
exports.AISkeletonPlaceholder = AISkeletonPlaceholder;
exports.AISkeletonIcon = AISkeletonIcon;

// ---------------------------------------------------------------- ChatButton
var CHAT_OWN = ['className', 'children', 'disabled', 'isQuickAction', 'isSelected', 'kind', 'renderIcon', 'size', 'type'];
var ChatButton = React.forwardRef(function ChatButton(props, ref) {
  var kind = props.kind || 'primary';
  var size = props.size || 'lg';
  if (props.isQuickAction) { kind = 'ghost'; size = 'sm'; }
  else if (['sm', 'md', 'lg'].indexOf(size) < 0) {
    if (typeof console !== 'undefined') console.error('Invalid size "' + size + '" provided to ChatButton. Size must be one of: sm, md, lg. Defaulting to "lg".');
    size = 'lg';
  }
  var rest = {};
  Object.keys(props).forEach(function (k) { if (CHAT_OWN.indexOf(k) < 0) rest[k] = props[k]; });
  var cls = cx('cds--btn', 'cds--btn--' + kind, 'cds--btn--' + size, props.className, 'cds--chat-btn',
    props.renderIcon && 'cds--chat-btn--with-icon',
    props.isQuickAction && 'cds--chat-btn--quick-action',
    props.isSelected && 'cds--chat-btn--quick-action--selected');
  var icon = props.renderIcon ? h(Icon, { name: props.renderIcon, size: 16, className: 'cds--btn__icon' }) : null;
  return h('button', Object.assign({}, rest, { ref: ref, type: props.type || 'button', disabled: props.disabled, className: cls }), props.children, icon);
});
ChatButton.displayName = 'ChatButton';
exports.ChatButton = ChatButton;

})(Carbon,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx);

/* ---- G2 ---- */
(function(exports,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx){
/* @components: FluidForm, FluidTextInput, FluidTextArea, FluidNumberInput, FluidSearch, FluidSelect, FluidTimePicker, FluidTimePickerSelect, FluidTextInputSkeleton, FluidTextAreaSkeleton, FluidNumberInputSkeleton, FluidSearchSkeleton, FluidSelectSkeleton, FluidTimePickerSkeleton */

// ---- shared helpers (local to this fragment) -------------------------------
function g2Id(id, prefix) { var auto = useId(); return id || (prefix + '-' + auto.replace(/:/g, '')); }

// WarningFilled (invalid) and WarningAltFilled (warn), as rendered by Carbon's useNormalizedInputProps.
function g2InvalidIcon(cls) { return h(Icon, { name: 'warning--filled', size: 16, className: cls }); }
var G2_WARN_ALT = '<path data-icon-path="inner-path" d="M16,26a1.5,1.5,0,1,1,1.5-1.5A1.5,1.5,0,0,1,16,26Zm-1.125-5h2.25V12h-2.25Z"/><path d="M16.002,6.1714h-.004L4.6487,27.9966,4.6506,28H27.3494l.0019-.0034ZM14.875,12h2.25v9h-2.25ZM16,26a1.5,1.5,0,1,1,1.5-1.5A1.5,1.5,0,0,1,16,26Z"/><path d="M29,30H3a1,1,0,0,1-.8872-1.4614l13-25a1,1,0,0,1,1.7744,0l13,25A1,1,0,0,1,29,30ZM4.6507,28H27.3493l.002-.0033L16.002,6.1714h-.004L4.6487,27.9967Z"/>';
function g2WarnIcon(cls) {
  return h('svg', { className: 'cds--icon ' + cls, width: 16, height: 16, viewBox: '0 0 32 32', focusable: 'false', 'aria-hidden': 'true', dangerouslySetInnerHTML: G2_HTML(G2_WARN_ALT) });
}
// Stable {__html} objects (shared htmlOf helper): React re-applies innerHTML when the object identity changes,
// which would swap the clicked <path> mid-click and swallow the click.
function G2_HTML(s) { return htmlOf(s); }
function g2Svg(cls, body) {
  return h('svg', { className: cls, width: 16, height: 16, viewBox: '0 0 16 16', focusable: 'false', 'aria-hidden': 'true', dangerouslySetInnerHTML: G2_HTML(body) });
}
var G2_ADD = '<path d="M9 7L9 3 7 3 7 7 3 7 3 9 7 9 7 13 9 13 9 9 13 9 13 7 9 7z"/>';
var G2_SUB = '<path d="M3 7H13V9H3z"/>';
var G2_CLOSE = '<path d="M12 4.7L11.3 4 8 7.3 4.7 4 4 4.7 7.3 8 4 11.3 4.7 12 8 8.7 11.3 12 12 11.3 8.7 8z"/>';

// Carbon useNormalizedInputProps: read-only and disabled fields never show invalid / warn.
function g2Norm(p) {
  var invalid = !p.readOnly && !p.disabled && !!p.invalid;
  var warn = !p.readOnly && !p.disabled && !invalid && !!p.warn;
  return { invalid: invalid, warn: warn };
}
// Uncontrolled/controlled value helper.
function g2Value(props, fallback) {
  var controlled = props.value !== undefined;
  var st = useState(props.defaultValue !== undefined ? props.defaultValue : fallback);
  return { controlled: controlled, value: controlled ? props.value : st[0], set: st[1] };
}

// ---- FluidForm --------------------------------------------------------------
function FluidForm(props) {
  var other = {};
  Object.keys(props).forEach(function (k) { if (k !== 'className' && k !== 'children') other[k] = props[k]; });
  return h('form', Object.assign({}, other, { className: cx('cds--form', 'cds--form--fluid', props.className) }), props.children);
}

// ---- FluidTextInput ---------------------------------------------------------
function FluidTextInput(props) {
  var id = g2Id(props.id, 'fluid-text-input');
  var disabled = !!props.disabled, readOnly = !!props.readOnly;
  var n = g2Norm(props), invalid = n.invalid, warn = n.warn;
  var v = g2Value(props, '');
  var isPassword = !!props.isPassword;
  var ps = useState(props.type === 'text' ? 'text' : 'password'); var pwType = ps[0], setPwType = ps[1];
  var text = v.value == null ? '' : String(v.value);
  function onChange(e) {
    if (disabled) return;
    if (!v.controlled) v.set(e.target.value);
    if (props.onChange) props.onChange(e);
  }
  var errId = id + '-error-msg', warnId = id + '-warn-msg';
  var msg = invalid ? h('div', { id: errId, className: 'cds--form-requirement' }, props.invalidText)
    : warn ? h('div', { id: warnId, className: 'cds--form-requirement' }, props.warnText) : null;
  var icon = invalid ? g2InvalidIcon('cds--text-input__invalid-icon')
    : warn ? g2WarnIcon('cds--text-input__invalid-icon cds--text-input__invalid-icon--warning') : null;
  var counter = !isPassword && props.enableCounter && props.maxCount
    ? h('div', { className: cx('cds--label', 'cds--text-input__label-counter', disabled && 'cds--label--disabled') }, text.length + '/' + props.maxCount) : null;
  var label = props.labelText ? h('label', { htmlFor: id, className: cx('cds--label', disabled && 'cds--label--disabled') }, props.labelText) : null;
  var visible = pwType === 'text';
  var hideLbl = props.hidePasswordLabel || 'Hide password', showLbl = props.showPasswordLabel || 'Show password';
  var input = h('input', {
    id: id, type: isPassword ? pwType : (props.type || 'text'),
    className: cx('cds--text-input', isPassword && 'cds--password-input', invalid && 'cds--text-input--invalid', warn && 'cds--text-input--warning'),
    placeholder: props.placeholder, title: props.placeholder, disabled: disabled, readOnly: readOnly, name: props.name,
    autoComplete: props.autoComplete, pattern: props.pattern, required: props.required,
    maxLength: !isPassword && props.enableCounter && props.maxCount ? props.maxCount : undefined,
    value: text, onChange: onChange, onBlur: props.onBlur, onFocus: props.onFocus, onKeyDown: props.onKeyDown,
    onClick: function (e) { if (!disabled && props.onClick) props.onClick(e); },
    'data-invalid': invalid || undefined, 'aria-invalid': invalid || undefined,
    'aria-describedby': invalid ? errId : warn ? warnId : undefined,
    'data-toggle-password-visibility': isPassword ? pwType === 'password' : undefined
  });
  var toggle = isPassword ? h('span', { className: 'cds--toggle-password-tooltip' },
    h('button', {
      type: 'button', className: 'cds--text-input--password__visibility__toggle', disabled: disabled,
      'aria-label': visible ? hideLbl : showLbl,
      onClick: function (e) { setPwType(visible ? 'password' : 'text'); if (props.onTogglePasswordVisibility) props.onTogglePasswordVisibility(e); }
    }, h(Icon, { name: visible ? 'view--off' : 'view', size: 16, className: visible ? 'cds--icon-visibility-off' : 'cds--icon-visibility-on' })),
    h('span', { className: 'cds--toggle-password-tooltip__content', role: 'tooltip', 'aria-hidden': 'true' }, visible ? hideLbl : showLbl)) : null;
  return h('div', {
    className: cx('cds--form-item', 'cds--text-input-wrapper', isPassword && 'cds--password-input-wrapper', 'cds--text-input--fluid', readOnly && 'cds--text-input-wrapper--readonly', props.className)
  },
    isPassword ? label : h('div', { className: 'cds--text-input__label-wrapper' }, label, counter),
    h('div', { className: 'cds--text-input__field-outer-wrapper' },
      h('div', { className: cx('cds--text-input__field-wrapper', warn && 'cds--text-input__field-wrapper--warning'), 'data-invalid': invalid || undefined },
        icon, input,
        h('hr', { className: 'cds--text-input__divider' }),
        toggle,
        msg)));
}

// ---- FluidTextArea ----------------------------------------------------------
function FluidTextArea(props) {
  var id = g2Id(props.id, 'fluid-text-area');
  var disabled = !!props.disabled, readOnly = !!props.readOnly;
  var n = g2Norm(props), invalid = n.invalid, warn = n.warn;
  var v = g2Value(props, '');
  var text = v.value == null ? '' : String(v.value);
  var mode = props.counterMode || 'character', maxCount = props.maxCount;
  var showCounter = !!(props.enableCounter && maxCount);
  var count = mode === 'word' ? ((text.match(/\S+/g) || []).length) : text.length;
  function onChange(e) {
    if (disabled) return;
    if (!v.controlled) v.set(e.target.value);
    if (props.onChange) props.onChange(e);
  }
  var errId = id + '-error-msg', warnId = id + '-warn-msg', cntId = id + '-counter-desc';
  return h('div', { className: cx('cds--form-item', 'cds--text-area--fluid', props.className) },
    h('div', { className: 'cds--text-area__label-wrapper' },
      props.labelText ? h('label', { htmlFor: id, className: cx('cds--label', disabled && 'cds--label--disabled') }, props.labelText) : null,
      showCounter ? h('div', { className: cx('cds--label', 'cds--text-area__label-counter', disabled && 'cds--label--disabled'), 'aria-hidden': 'true' }, count + '/' + maxCount) : null),
    showCounter ? h('span', { id: cntId, className: 'cds--visually-hidden' }, (mode === 'word' ? 'Word limit ' : 'Character limit ') + maxCount) : null,
    h('div', { className: cx('cds--text-area__wrapper', readOnly && 'cds--text-area__wrapper--readonly', warn && 'cds--text-area__wrapper--warn'), 'data-invalid': invalid || undefined },
      h('textarea', {
        id: id, className: cx('cds--text-area', invalid && 'cds--text-area--invalid', warn && 'cds--text-area--warn'),
        rows: props.rows || 4, placeholder: props.placeholder, disabled: disabled, readOnly: readOnly, name: props.name, required: props.required,
        maxLength: showCounter && mode === 'character' ? maxCount : undefined,
        value: text, onChange: onChange, onBlur: props.onBlur, onFocus: props.onFocus, onKeyDown: props.onKeyDown, onClick: props.onClick,
        'aria-invalid': invalid || undefined, 'aria-readonly': readOnly || undefined,
        'aria-describedby': cx(invalid ? errId : warn ? warnId : null, showCounter ? cntId : null) || undefined
      }),
      h('hr', { className: 'cds--text-area__divider' }),
      invalid ? h('div', { id: errId, role: 'alert', className: 'cds--form-requirement' }, props.invalidText, g2InvalidIcon('cds--text-area__invalid-icon')) : null,
      warn ? h('div', { id: warnId, role: 'alert', className: 'cds--form-requirement' }, props.warnText, g2WarnIcon('cds--text-area__invalid-icon cds--text-area__invalid-icon--warning')) : null));
}

// ---- FluidNumberInput -------------------------------------------------------
function g2Decimals(n) { var p = String(n).split('.'); return p[1] ? p[1].length : 0; }
function FluidNumberInput(props) {
  var id = g2Id(props.id, 'fluid-number-input');
  var disabled = !!props.disabled, readOnly = !!props.readOnly;
  var step = props.step == null ? 1 : props.step, min = props.min, max = props.max;
  var v = g2Value(props, props.allowEmpty ? '' : 0);
  var fs = useState(false); var focused = fs[0], setFocused = fs[1];
  var num = (v.value === '' || v.value == null) ? NaN : Number(v.value);
  var outOfRange = !isNaN(num) && ((min != null && num < min) || (max != null && num > max));
  var badEmpty = isNaN(num) && !props.allowEmpty;
  var n = g2Norm({ readOnly: readOnly, disabled: disabled, invalid: props.invalid || outOfRange || badEmpty, warn: props.warn });
  var invalid = n.invalid, warn = n.warn;
  function emit(e, val, dir) {
    if (!v.controlled) v.set(val);
    if (props.onChange) props.onChange(e, { value: val, direction: dir });
  }
  function onInput(e) {
    if (disabled || readOnly) return;
    var raw = e.target.value, val = raw === '' ? '' : Number(raw);
    emit(e, val, val !== '' && val < num ? 'down' : 'up');
  }
  function stepBy(e, dir) {
    var cur = isNaN(num) ? 0 : num;
    var prec = Math.max(g2Decimals(cur), g2Decimals(step));
    var val = parseFloat((dir === 'up' ? cur + step : cur - step).toFixed(prec));
    if (min != null) val = Math.max(val, min);
    if (max != null) val = Math.min(val, max);
    emit(e, val, dir);
    if (props.onClick) props.onClick(e, { value: val, direction: dir });
  }
  function onFocusChange(e) {
    // Carbon: focus on a stepper button does not count as field focus.
    if (e.target && e.target.type === 'button') setFocused(false);
    else setFocused(e.type === 'focus');
  }
  var msgId = id + (invalid ? '-error-msg' : '-warn-msg');
  var label = props.label || props.labelText;
  return h('div', {
    className: cx('cds--form-item', 'cds--number-input--fluid', invalid && 'cds--number-input--fluid--invalid', focused && 'cds--number-input--fluid--focus', disabled && 'cds--number-input--fluid--disabled', props.className),
    onFocus: onFocusChange, onBlur: onFocusChange
  },
    h('div', { className: cx('cds--number', readOnly && 'cds--number--readonly', props.hideSteppers && 'cds--number--nosteppers'), 'data-invalid': invalid || undefined },
      label ? h('label', { htmlFor: id, className: cx('cds--label', disabled && 'cds--label--disabled') }, label) : null,
      h('div', { className: cx('cds--number__input-wrapper', warn && 'cds--number__input-wrapper--warning') },
        h('input', {
          id: id, type: 'number', name: props.name, placeholder: props.placeholder, min: min, max: max, step: step,
          value: v.value == null ? '' : v.value, onChange: onInput, onBlur: props.onBlur, onFocus: props.onFocus, onKeyDown: props.onKeyDown,
          disabled: disabled, readOnly: readOnly, 'data-invalid': invalid || undefined,
          'aria-invalid': invalid || undefined, 'aria-readonly': readOnly || undefined, 'aria-describedby': invalid || warn ? msgId : undefined
        }),
        invalid ? g2InvalidIcon('cds--number__invalid') : null,
        warn ? g2WarnIcon('cds--number__invalid cds--number__invalid--warning') : null,
        props.hideSteppers ? null : h('div', { className: 'cds--number__controls' },
          h('button', { type: 'button', className: 'cds--number__control-btn down-icon', 'aria-label': props.decrementNumLabel || 'Decrement number', title: props.decrementNumLabel || 'Decrement number', tabIndex: -1, disabled: disabled || readOnly, onClick: function (e) { stepBy(e, 'down'); } }, g2Svg('down-icon', G2_SUB)),
          h('div', { className: 'cds--number__rule-divider' }),
          h('button', { type: 'button', className: 'cds--number__control-btn up-icon', 'aria-label': props.incrementNumLabel || 'Increment number', title: props.incrementNumLabel || 'Increment number', tabIndex: -1, disabled: disabled || readOnly, onClick: function (e) { stepBy(e, 'up'); } }, g2Svg('up-icon', G2_ADD)),
          h('div', { className: 'cds--number__rule-divider' }))),
      h('hr', { className: 'cds--number-input__divider' }),
      invalid ? h('div', { id: msgId, className: 'cds--form-requirement' }, props.invalidText || 'Number is not valid')
        : warn ? h('div', { id: msgId, className: 'cds--form-requirement' }, props.warnText) : null));
}

// ---- FluidSearch ------------------------------------------------------------
function FluidSearch(props) {
  var id = g2Id(props.id, 'fluid-search'); var searchId = id + '-search';
  var disabled = !!props.disabled;
  var inputRef = useRef(null);
  var v = g2Value(props, '');
  var text = v.value == null ? '' : String(v.value);
  var hasContent = text !== '';
  function onChange(e) { if (!v.controlled) v.set(e.target.value); if (props.onChange) props.onChange(e); }
  function clear() {
    if (!v.controlled) v.set('');
    if (props.onChange) props.onChange({ type: 'change', target: { value: '', id: id }, currentTarget: { value: '' } });
    if (props.onClear) props.onClear();
    if (inputRef.current) inputRef.current.focus();
  }
  function onKeyDown(e) {
    if (props.onKeyDown) props.onKeyDown(e);
    if (e.key === 'Escape' && hasContent) { e.stopPropagation(); clear(); }
  }
  var placeholder = props.placeholder == null ? '' : props.placeholder;
  var closeLabel = props.closeButtonLabelText || 'Clear search input';
  return h('div', { role: 'search', 'aria-label': placeholder || undefined, className: cx('cds--search', 'cds--search--fluid', disabled && 'cds--search--disabled', props.className) },
    h('div', { className: 'cds--search-magnifier' }, h(Icon, { name: 'search', size: 16, className: 'cds--search-magnifier-icon' })),
    h('label', { id: searchId, htmlFor: id, className: 'cds--label' }, props.labelText),
    h('input', {
      ref: inputRef, id: id, type: props.type || 'text', role: props.role || 'searchbox', className: 'cds--search-input', autoComplete: props.autoComplete || 'off',
      placeholder: placeholder, disabled: disabled, value: text, onChange: onChange, onKeyDown: onKeyDown, onBlur: props.onBlur, onFocus: props.onFocus, name: props.name
    }),
    h('button', { type: 'button', className: cx('cds--search-close', !hasContent && 'cds--search-close--hidden'), 'aria-label': closeLabel, title: closeLabel, disabled: disabled, tabIndex: hasContent ? undefined : -1, onClick: clear },
      g2Svg('cds--icon', G2_CLOSE)));
}

// ---- FluidSelect / FluidTimePickerSelect -----------------------------------
function FluidSelect(props) {
  var id = g2Id(props.id, 'fluid-select');
  var disabled = !!props.disabled, readOnly = !!props.readOnly;
  var n = g2Norm(props), invalid = n.invalid, warn = n.warn;
  var fs = useState(false); var focused = fs[0], setFocused = fs[1];
  var other = {};
  ['value', 'defaultValue', 'onChange', 'onBlur', 'onFocus', 'name', 'required', 'autoComplete'].forEach(function (k) { if (props[k] !== undefined) other[k] = props[k]; });
  var msgId = id + (invalid ? '-error-msg' : '-warn-msg');
  function onFocusChange(e) { setFocused(e.type === 'focus'); }
  return h('div', { className: cx('cds--form-item', 'cds--select--fluid', props.className) },
    h('div', { className: cx('cds--select', invalid && 'cds--select--invalid', disabled && 'cds--select--disabled', readOnly && 'cds--select--readonly', warn && 'cds--select--warning', invalid && 'cds--select--fluid--invalid', focused && 'cds--select--fluid--focus') },
      props.noLabel ? null : h('label', { htmlFor: id, className: cx('cds--label', disabled && 'cds--label--disabled') }, props.labelText),
      h('div', { className: 'cds--select-input__wrapper', 'data-invalid': invalid || undefined, onFocus: onFocusChange, onBlur: onFocusChange },
        h('select', Object.assign({}, other, {
          id: id, className: 'cds--select-input', disabled: disabled || undefined,
          'aria-invalid': invalid || undefined, 'aria-readonly': readOnly || undefined, 'aria-describedby': invalid || warn ? msgId : undefined,
          onMouseDown: function (e) { if (readOnly) { e.preventDefault(); e.target.focus(); } },
          onKeyDown: function (e) { if (readOnly && ['ArrowDown', 'ArrowUp', ' '].indexOf(e.key) > -1) e.preventDefault(); if (props.onKeyDown) props.onKeyDown(e); }
        }), props.children),
        h(Icon, { name: 'chevron--down', size: 16, className: 'cds--select__arrow' }),
        invalid ? g2InvalidIcon('cds--select__invalid-icon') : null,
        warn ? g2WarnIcon('cds--select__invalid-icon cds--select__invalid-icon--warning') : null,
        h('hr', { className: 'cds--select__divider' }),
        invalid ? h('div', { id: msgId, className: 'cds--form-requirement' }, props.invalidText)
          : warn ? h('div', { id: msgId, className: 'cds--form-requirement' }, props.warnText) : null)));
}
function FluidTimePickerSelect(props) { return h(FluidSelect, props); }

// ---- FluidTimePicker --------------------------------------------------------
function FluidTimePicker(props) {
  var disabled = !!props.disabled, readOnly = !!props.readOnly;
  // Disabled and read-only pickers never show invalid / warn (same rule as the other fluid inputs).
  var n = g2Norm(props), invalid = n.invalid, warn = n.warn;
  var kids = React.Children.toArray(props.children);
  var error = invalid || warn;
  var mapped = kids.map(function (c) {
    if (disabled) return React.cloneElement(c, { disabled: true });
    if (readOnly) return React.cloneElement(c, { readOnly: true });
    return c;
  });
  var inputProps = {};
  ['id', 'labelText', 'placeholder', 'value', 'defaultValue', 'onChange', 'onBlur', 'onFocus', 'onClick', 'onKeyDown', 'name', 'pattern', 'required', 'autoComplete'].forEach(function (k) { if (props[k] !== undefined) inputProps[k] = props[k]; });
  return h('div', {
    className: cx(props.className, 'cds--time-picker--fluid', kids.length !== 2 && 'cds--time-picker--equal-width', disabled && 'cds--time-picker--fluid--disabled', invalid && 'cds--time-picker--fluid--invalid', warn && 'cds--time-picker--fluid--warning')
  },
    h('div', { className: 'cds--time-picker--fluid__wrapper' },
      h('div', { className: 'cds--time-picker__input' },
        h(FluidTextInput, Object.assign({}, inputProps, { readOnly: readOnly, disabled: disabled }))),
      mapped),
    error ? h('hr', { className: 'cds--time-picker__divider' }) : null,
    error ? h('div', { className: 'cds--form-requirement' }, invalid ? props.invalidText : props.warnText) : null,
    !error ? null : invalid
      ? g2InvalidIcon('cds--time-picker__icon cds--time-picker__icon--invalid')
      : g2WarnIcon('cds--time-picker__icon cds--time-picker__icon--warn'));
}

// ---- Skeletons ----------------------------------------------------------------
// Carbon's Fluid*Skeleton components: a 64px skeleton field with a label bar and a value bar.
function g2SkelBars() {
  return [h('span', { key: 'l', className: 'cds--label cds--skeleton' }), h('div', { key: 'v', className: 'cds--skeleton cds--fluid__skeleton-value' })];
}
function g2Skel(cls) {
  return function (props) {
    return h('div', { className: cx('cds--form-item', 'cds--fluid__skeleton', cls, props.className) }, g2SkelBars());
  };
}
var FluidTextInputSkeleton = g2Skel('cds--text-input--fluid__skeleton');
var FluidTextAreaSkeleton = g2Skel('cds--text-area--fluid__skeleton');
var FluidNumberInputSkeleton = g2Skel('cds--number-input--fluid__skeleton');
var FluidSearchSkeleton = g2Skel('cds--search--fluid__skeleton');
var FluidSelectSkeleton = g2Skel('cds--select--fluid__skeleton');
function FluidTimePickerSkeleton(props) {
  var parts = props.isOnlyTwo ? 2 : 3;
  var kids = [];
  for (var i = 0; i < parts; i++) kids.push(h('div', { key: i, className: 'cds--fluid__skeleton cds--time-picker--fluid__skeleton-part' }, g2SkelBars()));
  return h('div', { className: cx('cds--time-picker--fluid--skeleton', props.isOnlyTwo && 'cds--time-picker--equal-width', props.className) },
    h('div', { className: 'cds--time-picker--fluid__wrapper' }, kids));
}

exports.FluidForm = FluidForm;
exports.FluidTextInput = FluidTextInput;
exports.FluidTextArea = FluidTextArea;
exports.FluidNumberInput = FluidNumberInput;
exports.FluidSearch = FluidSearch;
exports.FluidSelect = FluidSelect;
exports.FluidTimePicker = FluidTimePicker;
exports.FluidTimePickerSelect = FluidTimePickerSelect;
exports.FluidTextInputSkeleton = FluidTextInputSkeleton;
exports.FluidTextAreaSkeleton = FluidTextAreaSkeleton;
exports.FluidNumberInputSkeleton = FluidNumberInputSkeleton;
exports.FluidSearchSkeleton = FluidSearchSkeleton;
exports.FluidSelectSkeleton = FluidSelectSkeleton;
exports.FluidTimePickerSkeleton = FluidTimePickerSkeleton;

})(Carbon,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx);

/* ---- G3 ---- */
(function(exports,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx){
/* @components: FluidDropdown, FluidComboBox, FluidMultiSelect, FluidDatePicker, FluidDatePickerInput, FluidDropdownSkeleton, FluidComboBoxSkeleton, FluidMultiSelectSkeleton, FluidDatePickerSkeleton */
var Fragment = React.Fragment;

// WarningAltFilled as Carbon draws it: yellow triangle, dark "!" (inner path), 16px.
var G3_WARN_ALT = '<path data-icon-path="inner-path" d="M16,26a1.5,1.5,0,1,1,1.5-1.5A1.5,1.5,0,0,1,16,26Zm-1.125-5h2.25V12h-2.25Z"/><path d="M16.002,6.1714h-.004L4.6487,27.9966,4.6506,28H27.3494l.0019-.0034ZM14.875,12h2.25v9h-2.25ZM16,26a1.5,1.5,0,1,1,1.5-1.5A1.5,1.5,0,0,1,16,26Z"/><path d="M29,30H3a1,1,0,0,1-.8872-1.4614l13-25a1,1,0,0,1,1.7744,0l13,25A1,1,0,0,1,29,30ZM4.6507,28H27.3493l.002-.0033L16.002,6.1714h-.004L4.6487,27.9967Z"/>';
function g3WarnIcon(cls) {
  return h('svg', { className: 'cds--icon ' + cls, width: 16, height: 16, viewBox: '0 0 32 32', focusable: 'false', 'aria-hidden': 'true', dangerouslySetInnerHTML: htmlOf(G3_WARN_ALT) });
}

/* ===================================================================== */
/* Fluid list-box family: the round-2 Dropdown / ComboBox / MultiSelect   */
/* render the same DOM as Carbon (label is a sibling before .cds--list-box */
/* inside .cds--list-box__wrapper), so the fluid variants only add the    */
/* wrapper classes Carbon adds under FormContext isFluid and drop the     */
/* props fluid inputs do not support (size, inline type, helper text).   */
/* ===================================================================== */
function flProps(props, extraCls) {
  var o = {};
  for (var k in props) { if (Object.prototype.hasOwnProperty.call(props, k)) o[k] = props[k]; }
  delete o.isCondensed; delete o.isFilterable; delete o.helperText; delete o.size; delete o.type; delete o.hideLabel;
  // Carbon useNormalizedInputProps: disabled and read-only fields never show invalid / warn.
  var blocked = !!(props.disabled || props.readOnly);
  o.invalid = !blocked && !!props.invalid;
  o.warn = !blocked && !o.invalid && !!props.warn;
  o.className = cx(
    'cds--list-box__wrapper--fluid',
    extraCls,
    o.invalid && 'cds--list-box__wrapper--fluid--invalid',
    props.isCondensed && 'cds--list-box__wrapper--fluid--condensed',
    props.className);
  return o;
}

function FluidDropdown(props) {
  var D = exports.Dropdown;
  return D ? h(D, flProps(props)) : null;
}
exports.FluidDropdown = FluidDropdown;

function FluidComboBox(props) {
  var D = exports.ComboBox;
  return D ? h(D, flProps(props)) : null;
}
exports.FluidComboBox = FluidComboBox;

function FluidMultiSelect(props) {
  var D = exports.MultiSelect;
  if (!D) return null;
  var filterable = !!(props.isFilterable || props.filterable);
  var o = flProps(props, filterable && 'cds--multi-select--filterable__wrapper');
  o.filterable = filterable;
  return h(D, o);
}
exports.FluidMultiSelect = FluidMultiSelect;

/* ===================================================================== */
/* FluidDatePicker + FluidDatePickerInput                                */
/* Carbon composes <FluidDatePicker><FluidDatePickerInput/>...; the fluid */
/* input markup (two icons, container-level invalid classes, picker-level */
/* error row) differs from the round-2 DatePicker, so the calendar logic  */
/* is carried here and reuses the round-2 .flatpickr-* calendar styles.   */
/* ===================================================================== */
var FDP_MONTHS = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
var FDP_WD = ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'];
var FDP_WD_LONG = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
function fdpStrip(d) { return new Date(d.getFullYear(), d.getMonth(), d.getDate()); }
function fdpSame(a, b) { return !!(a && b && a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate()); }
function fdpPad(n, l) { var s = String(n); while (s.length < l) s = '0' + s; return s; }
function fdpFormat(d, fmt) {
  if (!d) return '';
  return fmt.replace(/[mdjnYy]/g, function (t) {
    switch (t) {
      case 'm': return fdpPad(d.getMonth() + 1, 2);
      case 'n': return String(d.getMonth() + 1);
      case 'd': return fdpPad(d.getDate(), 2);
      case 'j': return String(d.getDate());
      case 'Y': return String(d.getFullYear());
      default: return fdpPad(d.getFullYear() % 100, 2);
    }
  });
}
function fdpParse(str, fmt) {
  if (str == null) return null;
  var order = [];
  var src = '^\\s*' + fmt.replace(/[mdjnYy]|[^mdjnYy]+/g, function (t) {
    if (/^[mdjnYy]$/.test(t)) { order.push(t); return '(\\d{1,4})'; }
    return t.replace(/[.*+?^()|[\]\\\/{}]/g, function (c) { return '\\' + c; });
  }) + '\\s*' + String.fromCharCode(36);
  var m = new RegExp(src).exec(String(str));
  if (!m) return null;
  var y = null, mo = null, d = null;
  for (var i = 0; i < order.length; i++) {
    var v = parseInt(m[i + 1], 10), t = order[i];
    if (t === 'm' || t === 'n') mo = v; else if (t === 'd' || t === 'j') d = v; else if (t === 'Y') y = v; else y = 2000 + v;
  }
  if (y == null || mo == null || d == null || mo < 1 || mo > 12 || d < 1 || y < 1000) return null;
  var date = new Date(y, mo - 1, d);
  return date.getMonth() === mo - 1 && date.getDate() === d ? date : null;
}
function fdpToDate(v, fmt) {
  if (v == null || v === '') return null;
  if (v instanceof Date) return isNaN(v.getTime()) ? null : fdpStrip(v);
  if (typeof v === 'number') return fdpStrip(new Date(v));
  var p = fdpParse(v, fmt); if (p) return p;
  var iso = /^(\d{4})-(\d{2})-(\d{2})/.exec(String(v));
  if (iso) return new Date(+iso[1], +iso[2] - 1, +iso[3]);
  var g = new Date(v); return isNaN(g.getTime()) ? null : fdpStrip(g);
}
function fdpToList(v, fmt) {
  if (v === undefined || v === null) return [];
  var arr = Object.prototype.toString.call(v) === '[object Array]' ? v : [v];
  var out = [];
  for (var i = 0; i < arr.length; i++) { var d = fdpToDate(arr[i], fmt); if (d) out.push(d); }
  return out;
}
function fdpTitle(d) { return FDP_WD_LONG[d.getDay()] + ', ' + FDP_MONTHS[d.getMonth()] + ' ' + d.getDate() + ', ' + d.getFullYear(); }
function fdpCall(f, e) { if (typeof f === 'function') f(e); }

function FluidDatePickerInput(props) {
  var dp = props.__dp || null;
  var autoId = useId(); var id = props.id || ('fdpi-' + autoId);
  var type = (dp && dp.type) || props.datePickerType || 'single';
  var disabled = !!(props.disabled || (dp && dp.disabled));
  var readOnly = !!(props.readOnly || (dp && dp.readOnly));
  // input-level validation (one input of a range, or a lone input)
  // Carbon useNormalizedInputProps: disabled and read-only inputs never show invalid / warn.
  var invalid = !disabled && !readOnly && !!props.invalid, warn = !disabled && !readOnly && !invalid && !!props.warn;
  // picker-level validation styles the inputs; the message lives on the picker
  var gInvalid = !!(dp && dp.invalid), gWarn = !gInvalid && !!(dp && dp.warn);
  var errId = id + '-err', warnId = id + '-warn';
  var describedBy = invalid ? errId : warn ? warnId : (dp && dp.describedBy) || undefined;
  var statusIcon = null;
  if (!disabled && !readOnly) {
    if (invalid) statusIcon = h(Icon, { name: 'warning--filled', size: 16, className: 'cds--date-picker__icon cds--date-picker__icon--invalid' });
    else if (warn) statusIcon = g3WarnIcon('cds--date-picker__icon cds--date-picker__icon--warn');
  }
  var inputProps = {
    ref: dp ? dp.inputRef : props.inputRef, id: id, type: 'text', autoComplete: 'off',
    className: cx('cds--date-picker__input', (invalid || gInvalid) && 'cds--date-picker__input--invalid', (warn || gWarn) && 'cds--date-picker__input--warn'),
    placeholder: props.placeholder, pattern: props.pattern, disabled: disabled, readOnly: readOnly,
    'data-invalid': (invalid || gInvalid) || undefined, 'aria-invalid': (invalid || gInvalid) || undefined, 'aria-describedby': describedBy,
    style: props.style,
    onChange: function (e) { if (disabled) return; if (dp) dp.onChange(e); fdpCall(props.onChange, e); },
    onClick: function (e) { if (disabled) return; if (dp) dp.onClick(e); fdpCall(props.onClick, e); },
    onBlur: function (e) { if (dp) dp.onBlur(e); fdpCall(props.onBlur, e); },
    onFocus: props.onFocus,
    onKeyDown: function (e) { if (dp) dp.onKeyDown(e); fdpCall(props.onKeyDown, e); }
  };
  if (dp) inputProps.value = dp.value;
  else if (props.value !== undefined) inputProps.value = props.value;
  else inputProps.defaultValue = props.defaultValue;
  return h('div', { className: cx('cds--date-picker-container', !props.labelText && 'cds--date-picker--nolabel', invalid && 'cds--date-picker--fluid--invalid', warn && 'cds--date-picker--fluid--warn', props.className) },
    props.labelText ? h('label', { htmlFor: id, className: cx('cds--label', disabled && 'cds--label--disabled', readOnly && 'cds--label--readonly') }, props.labelText) : null,
    h('div', { className: cx('cds--date-picker-input__wrapper', invalid && 'cds--date-picker-input__wrapper--invalid', warn && 'cds--date-picker-input__wrapper--warn') },
      h('span', null,
        h('input', inputProps),
        type !== 'simple' ? h(Icon, { name: 'calendar', size: 16, className: 'cds--date-picker__icon' }) : null,
        statusIcon),
      dp ? dp.calendar : null),
    invalid ? h('div', { id: errId, className: 'cds--form-requirement' }, props.invalidText) : null,
    warn ? h('div', { id: warnId, className: 'cds--form-requirement' }, props.warnText) : null);
}
exports.FluidDatePickerInput = FluidDatePickerInput;

function FluidDatePicker(props) {
  var type = props.datePickerType || 'single';
  var fmt = props.dateFormat || 'm/d/Y';
  var range = type === 'range', simple = type === 'simple';
  var minD = fdpToDate(props.minDate, fmt), maxD = fdpToDate(props.maxDate, fmt);
  var autoId = useId(); var baseId = props.id || ('fdp-' + autoId);
  var ctl = props.value !== undefined;
  var ds = useState(fdpToList(props.defaultValue, fmt));
  var dates = ctl ? fdpToList(props.value, fmt) : ds[0];
  var dkey = dates.map(function (d) { return d.getTime(); }).join(',');
  function texts(list) { return [fdpFormat(list[0], fmt), fdpFormat(list[1], fmt)]; }
  var ts = useState(texts(dates)), text = ts[0], setText = ts[1];
  var firstSync = useRef(true);
  useEffect(function () { if (firstSync.current) { firstSync.current = false; return; } setText(texts(dates)); }, [dkey, fmt]);

  var today = fdpStrip(new Date());
  var blocked = props.disabled || props.readOnly || simple;
  var os = useState(!!props.defaultOpen);
  var openCtl = props.open !== undefined;
  var open = (openCtl ? !!props.open : os[0]) && !blocked;
  function setOpen(v) { if (!openCtl) os[1](v); if (props.onOpenChange) props.onOpenChange(v); }
  var anchor = dates[0] || today;
  var vs = useState({ y: anchor.getFullYear(), m: anchor.getMonth() }), view = vs[0], setView = vs[1];
  var fs = useState(anchor), fd = fs[0], setFd = fs[1];
  var keyNav = useRef(false), calRef = useRef(null), rootRef = useRef(null), inputRef = useRef(null);
  var hs = useState(null), hover = hs[0], setHover = hs[1];

  var outCb = useRef(null); outCb.current = function () { setOpen(false); };
  useEffect(function () {
    if (!open) return undefined;
    function f(e) { if (rootRef.current && !rootRef.current.contains(e.target)) outCb.current(e); }
    document.addEventListener('mousedown', f);
    return function () { document.removeEventListener('mousedown', f); };
  }, [open]);
  useEffect(function () {
    if (!open || !keyNav.current || !calRef.current) return;
    var el = calRef.current.querySelector('[data-ts="' + fd.getTime() + '"]');
    if (el) { el.focus(); keyNav.current = false; }
  }, [fd, open, view.y, view.m]);

  function commit(list) {
    if (!ctl) ds[1](list);
    setText(texts(list));
    if (props.onChange) props.onChange(list);
  }
  function isOff(d) { return !!((minD && d < minD) || (maxD && d > maxD)); }
  function pick(d) {
    if (isOff(d)) return;
    setFd(d);
    if (!range) { commit([d]); setOpen(false); if (inputRef.current) inputRef.current.focus(); return; }
    if (dates.length !== 1) { commit([d]); return; }
    var s = dates[0];
    if (d < s) commit([d, s]); else commit([s, d]);
    setOpen(false);
    if (inputRef.current) inputRef.current.focus();
  }
  function go(y, m, keepDay) {
    var dt = new Date(y, m, 1); y = dt.getFullYear(); m = dt.getMonth();
    setView({ y: y, m: m });
    if (keepDay) { var last = new Date(y, m + 1, 0).getDate(); setFd(new Date(y, m, Math.min(fd.getDate(), last))); }
  }
  function moveFocus(d) {
    keyNav.current = true; setFd(d);
    if (d.getFullYear() !== view.y || d.getMonth() !== view.m) setView({ y: d.getFullYear(), m: d.getMonth() });
  }
  function onDayKey(e) {
    var k = e.key, d = fd, n = null;
    if (k === 'ArrowRight') n = new Date(d.getFullYear(), d.getMonth(), d.getDate() + 1);
    else if (k === 'ArrowLeft') n = new Date(d.getFullYear(), d.getMonth(), d.getDate() - 1);
    else if (k === 'ArrowDown') n = new Date(d.getFullYear(), d.getMonth(), d.getDate() + 7);
    else if (k === 'ArrowUp') n = new Date(d.getFullYear(), d.getMonth(), d.getDate() - 7);
    else if (k === 'PageDown' || k === 'PageUp') { var mm = d.getMonth() + (k === 'PageDown' ? 1 : -1); var last = new Date(d.getFullYear(), mm + 1, 0).getDate(); n = new Date(d.getFullYear(), mm, Math.min(d.getDate(), last)); }
    else if (k === 'Home') n = new Date(d.getFullYear(), d.getMonth(), 1);
    else if (k === 'End') n = new Date(d.getFullYear(), d.getMonth() + 1, 0);
    else if (k === 'Escape') { e.preventDefault(); e.stopPropagation(); setOpen(false); if (inputRef.current) inputRef.current.focus(); return; }
    if (n) { e.preventDefault(); moveFocus(n); }
  }
  function openCal() {
    if (blocked) return;
    var a = dates[0] || today;
    if (!open) { setView({ y: a.getFullYear(), m: a.getMonth() }); setFd(a); }
    setOpen(true);
  }
  function applyText(idx) {
    var s = text[idx];
    if (!s) { var l = dates.filter(function (d, i) { return i !== idx; }); if (dates.length > idx) commit(l); return; }
    var d = fdpParse(s, fmt);
    if (!d || isOff(d)) { setText(texts(dates)); return; }
    var next = range ? (idx === 0 ? [d].concat(dates.slice(1)) : [dates[0] || d, d]) : [d];
    if (range && next.length === 2 && next[1] < next[0]) next = [next[1], next[0]];
    setView({ y: d.getFullYear(), m: d.getMonth() }); setFd(d);
    if (next.length === dates.length && next.every(function (x, i) { return fdpSame(x, dates[i]); })) { setText(texts(dates)); return; }
    commit(next);
  }
  function handlers(idx) {
    return {
      onChange: function (e) {
        var v = e.target.value, nt = text.slice(); nt[idx] = v; setText(nt);
        var d = fdpParse(v, fmt);
        if (d && !isOff(d)) { setView({ y: d.getFullYear(), m: d.getMonth() }); setFd(d); }
      },
      onBlur: function () { applyText(idx); },
      onClick: openCal,
      onKeyDown: function (e) {
        if (e.key === 'ArrowDown' && !blocked) {
          e.preventDefault(); keyNav.current = true;
          if (open) { var el = calRef.current && calRef.current.querySelector('[data-ts="' + fd.getTime() + '"]'); if (el) el.focus(); } else openCal();
        } else if (e.key === 'Escape' && open) { e.stopPropagation(); setOpen(false); }
        else if (e.key === 'Enter') applyText(idx);
      }
    };
  }

  // calendar ---------------------------------------------------------------
  var calendar = null;
  if (open) {
    var first = new Date(view.y, view.m, 1), lead = first.getDay();
    var cells = [];
    for (var i = 0; i < 42; i++) cells.push(new Date(view.y, view.m, 1 - lead + i));
    var s0 = dates[0], s1 = dates[1];
    var preview = range && dates.length === 1 && hover && !isOff(hover) ? hover : null;
    var rs = s0, re = s1 || preview; if (rs && re && re < rs) { var tmp = rs; rs = re; re = tmp; }
    var fdVisible = cells.some(function (c) { return fdpSame(c, fd); });
    var prevOff = !!(minD && new Date(view.y, view.m, 0) < minD), nextOff = !!(maxD && new Date(view.y, view.m + 1, 1) > maxD);
    calendar = h('div', { ref: calRef, className: 'flatpickr-calendar open animate', role: 'dialog', 'aria-label': 'Choose date', tabIndex: -1 },
      h('div', { className: 'flatpickr-months' },
        h('button', { type: 'button', className: cx('flatpickr-prev-month', prevOff && 'disabled'), 'aria-label': 'Previous month', disabled: prevOff, onClick: function () { go(view.y, view.m - 1, true); } }, h(Icon, { name: 'chevron--left', size: 16 })),
        h('div', { className: 'flatpickr-month' },
          h('div', { className: 'flatpickr-current-month' },
            h('span', { className: 'cur-month', 'aria-live': 'polite' }, FDP_MONTHS[view.m]),
            h('div', { className: 'numInputWrapper' },
              h('input', {
                className: 'numInput cur-year', type: 'number', 'aria-label': 'Year', value: view.y, min: 1000, max: 9999,
                onChange: function (e) { var y = parseInt(e.target.value, 10); if (y > 0) setView({ y: y, m: view.m }); }
              }),
              h('span', { className: 'arrowUp', 'aria-hidden': 'true', onClick: function () { go(view.y + 1, view.m, true); } }),
              h('span', { className: 'arrowDown', 'aria-hidden': 'true', onClick: function () { go(view.y - 1, view.m, true); } })))),
        h('button', { type: 'button', className: cx('flatpickr-next-month', nextOff && 'disabled'), 'aria-label': 'Next month', disabled: nextOff, onClick: function () { go(view.y, view.m + 1, true); } }, h(Icon, { name: 'chevron--right', size: 16 }))),
      h('div', { className: 'flatpickr-innerContainer' },
        h('div', { className: 'flatpickr-rContainer' },
          h('div', { className: 'flatpickr-weekdays' }, h('div', { className: 'flatpickr-weekdaycontainer' }, FDP_WD.map(function (w, j) { return h('span', { key: j, className: 'flatpickr-weekday', title: FDP_WD_LONG[j] }, w); }))),
          h('div', { className: 'flatpickr-days' },
            h('div', { className: 'dayContainer', role: 'group', 'aria-label': FDP_MONTHS[view.m] + ' ' + view.y, onKeyDown: onDayKey, onMouseLeave: function () { setHover(null); } },
              cells.map(function (d) {
                var out = d.getMonth() !== view.m, off = isOff(d);
                var isSel = fdpSame(d, s0) || fdpSame(d, s1);
                var inR = rs && re && d > rs && d < re;
                var tab = fdVisible ? fdpSame(d, fd) : (!out && d.getDate() === 1);
                return h('button', {
                  key: d.getTime(), type: 'button', 'data-ts': d.getTime(), tabIndex: tab ? 0 : -1, disabled: off, 'aria-label': fdpTitle(d), 'aria-pressed': isSel,
                  className: cx('flatpickr-day', out && (d < first ? 'prevMonthDay' : 'nextMonthDay'), fdpSame(d, today) && 'today', isSel && 'selected', inR && 'inRange', range && rs && re && fdpSame(d, rs) && 'startRange', range && rs && re && fdpSame(d, re) && 'endRange', off && 'flatpickr-disabled'),
                  onClick: function () { pick(d); }, onMouseEnter: function () { if (range && dates.length === 1) setHover(d); }
                }, d.getDate());
              }))))));
  }

  // inputs: FluidDatePickerInput children, or defaults built from props ------
  var kids = [];
  React.Children.forEach(props.children, function (c) { if (React.isValidElement(c)) kids.push(c); });
  var ph = props.placeholder || fmt.replace(/[mdjnYy]/g, function (t) { return { m: 'mm', n: 'mm', d: 'dd', j: 'dd', Y: 'yyyy', y: 'yy' }[t]; });
  if (!kids.length) {
    kids.push(h(FluidDatePickerInput, { key: 's', id: baseId + '-start', labelText: props.labelText || (range ? 'Start date' : 'Date'), placeholder: ph }));
    if (range) kids.push(h(FluidDatePickerInput, { key: 'e', id: baseId + '-end', labelText: props.endLabelText || 'End date', placeholder: props.endPlaceholder || ph }));
  }
  var count = range ? Math.min(kids.length, 2) : 1;
  var gBlocked = !!(props.disabled || props.readOnly);
  var gInvalid = !gBlocked && !!props.invalid, gWarn = !gBlocked && !gInvalid && !!props.warn;
  var msgId = baseId + '-msg';
  var inputs = [];
  for (var k = 0; k < count; k++) {
    var hd = handlers(k);
    inputs.push(React.cloneElement(kids[k], {
      key: kids[k].key != null ? kids[k].key : 'i' + k,
      placeholder: kids[k].props.placeholder != null ? kids[k].props.placeholder : (k === 1 && props.endPlaceholder ? props.endPlaceholder : ph),
      __dp: {
        type: type, value: text[k], onChange: hd.onChange, onBlur: hd.onBlur, onClick: hd.onClick, onKeyDown: hd.onKeyDown,
        inputRef: k === 0 ? inputRef : undefined, calendar: k === 0 ? calendar : null,
        disabled: props.disabled, readOnly: props.readOnly, invalid: gInvalid, warn: gWarn, describedBy: (gInvalid || gWarn) ? msgId : undefined
      }
    }));
  }
  var fluidError = null;
  if (gInvalid || gWarn) {
    fluidError = h(Fragment, null,
      gInvalid ? h(Icon, { name: 'warning--filled', size: 16, className: 'cds--date-picker__icon cds--date-picker__icon--invalid' }) : g3WarnIcon('cds--date-picker__icon cds--date-picker__icon--warn'),
      h('hr', { className: 'cds--date-picker__divider' }),
      h('div', { id: msgId, className: 'cds--form-requirement' }, gInvalid ? props.invalidText : props.warnText));
  }
  return h('div', {
    ref: rootRef, style: props.style,
    className: cx('cds--form-item', 'cds--date-picker--fluid', gInvalid && 'cds--date-picker--fluid--invalid', props.readOnly && 'cds--date-picker--fluid--readonly', gWarn && 'cds--date-picker--fluid--warn', props.className)
  },
    h('div', { className: cx('cds--date-picker', 'cds--date-picker--' + type) }, inputs),
    fluidError);
}
exports.FluidDatePicker = FluidDatePicker;

/* ===================================================================== */
/* Skeletons: a 64px skeleton field with a label bar and a value bar      */
/* ===================================================================== */
function g3SkelField(key, extra) {
  return h('div', { key: key, className: cx('cds--fluid__skeleton', extra) },
    h('span', { className: 'cds--label cds--skeleton' }),
    h('div', { className: 'cds--skeleton cds--fluid__skeleton-value' }));
}
function g3ListSkel(cls) {
  return function (props) {
    return h('div', { className: cx('cds--list-box__wrapper--fluid__skeleton', cls, props.className) }, g3SkelField('f'));
  };
}
var FluidDropdownSkeleton = g3ListSkel('cds--dropdown--fluid__skeleton');
var FluidComboBoxSkeleton = g3ListSkel('cds--combo-box--fluid__skeleton');
var FluidMultiSelectSkeleton = g3ListSkel('cds--multi-select--fluid__skeleton');
function FluidDatePickerSkeleton(props) {
  var type = props.datePickerType || 'single';
  var kids = [g3SkelField('a')];
  if (type === 'range') kids.push(g3SkelField('b'));
  return h('div', { className: cx('cds--date-picker--fluid__skeleton', 'cds--date-picker--fluid__skeleton--' + type, props.className) }, kids);
}
exports.FluidDropdownSkeleton = FluidDropdownSkeleton;
exports.FluidComboBoxSkeleton = FluidComboBoxSkeleton;
exports.FluidMultiSelectSkeleton = FluidMultiSelectSkeleton;
exports.FluidDatePickerSkeleton = FluidDatePickerSkeleton;

})(Carbon,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx);

/* ---- G4 ---- */
(function(exports,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx){
/* @components: Form, FormGroup, FormLabel, FormItem, CheckboxGroup, Heading, Section, Text, TextDirection, Theme, GlobalTheme */
function g4Omit(props, names) { var o = {}; for (var k in props) { if (names.indexOf(k) < 0) o[k] = props[k]; } return o; }

// Form ----------------------------------------------------------------------
// packages/react/src/components/Form: a plain <form class="cds--form">.
function Form(props) {
  return h('form', Object.assign({}, g4Omit(props, ['className', 'children']), { className: cx('cds--form', props.className) }), props.children);
}
exports.Form = Form;

// FormItem: column flex wrapper used by every Carbon input.
function FormItem(props) {
  return h('div', Object.assign({}, g4Omit(props, ['className', 'children']), { className: cx('cds--form-item', props.className) }), props.children);
}
exports.FormItem = FormItem;

// FormGroup: <fieldset class="cds--fieldset"> + <legend class="cds--label">, optional message.
function FormGroup(props) {
  var autoId = useId();
  var rest = g4Omit(props, ['disabled', 'legendId', 'legendText', 'invalid', 'children', 'className', 'message', 'messageText']);
  var legendId = props.legendId || rest['aria-labelledby'] || ('fg-legend-' + autoId);
  var msgId = props.message ? 'fg-msg-' + autoId : undefined;
  var attrs = Object.assign({
    disabled: !!props.disabled,
    className: cx('cds--fieldset', props.className),
    'aria-describedby': msgId
  }, rest, { 'aria-labelledby': rest['aria-labelledby'] || legendId });
  if (props.invalid) attrs['data-invalid'] = '';
  return h('fieldset', attrs,
    h('legend', { className: 'cds--label', id: legendId }, props.legendText),
    props.children,
    props.message ? h('div', { id: msgId, className: 'cds--form__requirements' }, props.messageText) : null);
}
exports.FormGroup = FormGroup;

// Text / TextDirection ----------------------------------------------------------
// packages/react/src/components/Text: renders `as` (default span) with a resolved `dir`.
var TextDirectionContext = React.createContext(null);
function g4TextOf(children) {
  if (typeof children === 'string') return children;
  var out = [];
  React.Children.forEach(children, function (c) { if (typeof c === 'string') out.push(c); });
  return out.length === 1 ? out[0] : out;
}
var Text = React.forwardRef(function Text(props, ref) {
  var ctx = React.useContext(TextDirectionContext);
  var dir = props.dir || 'auto';
  var Base = props.as || 'span';
  var rest = g4Omit(props, ['as', 'children', 'dir']);
  var textProps = {};
  var value = Object.assign({}, ctx || {});
  if (!ctx) {
    textProps.dir = dir; value.direction = dir;
  } else {
    var parentDir = ctx.direction, getDir = ctx.getTextDirection;
    if (getDir && getDir.current) {
      var override = getDir.current(g4TextOf(props.children));
      if (parentDir !== override) { textProps.dir = override; value.direction = override; }
      else if (parentDir === 'auto') { textProps.dir = override; }
    } else if (parentDir !== dir) { textProps.dir = dir; value.direction = dir; }
    else if (parentDir === 'auto') { textProps.dir = dir; }
  }
  return h(TextDirectionContext.Provider, { value: value },
    h(Base, Object.assign({ ref: ref }, rest, textProps), props.children));
});
exports.Text = Text;

function TextDirection(props) {
  var saved = useRef(props.getTextDirection);
  var dir = props.dir || 'auto';
  var value = React.useMemo(function () { return { direction: dir, getTextDirection: saved }; }, [dir]);
  useEffect(function () { saved.current = props.getTextDirection; });
  return h(TextDirectionContext.Provider, { value: value }, props.children);
}
exports.TextDirection = TextDirection;

// FormLabel: <label class="cds--label cds--label--no-margin"> rendered through Text.
function FormLabel(props) {
  var rest = g4Omit(props, ['className', 'children', 'id']);
  return h(Text, Object.assign({ as: 'label', htmlFor: props.id }, rest, { className: cx('cds--label', 'cds--label--no-margin', props.className) }), props.children);
}
exports.FormLabel = FormLabel;

// CheckboxGroup -------------------------------------------------------------
// packages/react/src/components/CheckboxGroup: fieldset.cds--checkbox-group around Checkbox children.
function g4WarnAltIcon(cls) {
  return h('svg', {
    className: 'cds--icon ' + cls, width: 16, height: 16, viewBox: '0 0 16 16', focusable: 'false', 'aria-hidden': 'true',
    dangerouslySetInnerHTML: htmlOf('<path fill-rule="evenodd" d="M8 1L15.5 14.5H0.5L8 1ZM7.5 6V10.5H8.5V6H7.5ZM8 11.4C7.55 11.4 7.2 11.75 7.2 12.2C7.2 12.65 7.55 13 8 13C8.45 13 8.8 12.65 8.8 12.2C8.8 11.75 8.45 11.4 8 11.4Z"/>')
  });
}
function CheckboxGroup(props) {
  var autoId = useId();
  var readOnly = !!props.readOnly, invalid = !!props.invalid, warn = !!props.warn;
  var orientation = props.orientation || 'vertical';
  var showWarning = !readOnly && !invalid && warn;
  var showHelper = !invalid && !warn;
  var helperId = props.helperText ? 'checkbox-group-helper-text-' + autoId : undefined;
  var legendId = props.legendId || props['aria-labelledby'] || ('checkbox-group-legend-' + autoId);
  var rest = g4Omit(props, ['children', 'className', 'decorator', 'helperText', 'invalid', 'invalidText', 'legendId', 'legendText', 'readOnly', 'warn', 'warnText', 'orientation', 'disabled', 'onClickCapture']);
  var cls = cx('cds--checkbox-group', props.className,
    orientation === 'horizontal' && 'cds--checkbox-group--horizontal',
    readOnly && 'cds--checkbox-group--readonly',
    !readOnly && invalid && 'cds--checkbox-group--invalid',
    showWarning && 'cds--checkbox-group--warning',
    props.decorator && 'cds--checkbox-group--decorator');
  // readOnly: the boxes keep focus and are announced, but clicks (mouse, label, Space) do not toggle them.
  function onClickCapture(e) {
    if (readOnly && e.target && e.target.type === 'checkbox') e.preventDefault();
    if (props.onClickCapture) props.onClickCapture(e);
  }
  var helper = props.helperText ? h('div', { id: helperId, className: 'cds--form__helper-text' }, props.helperText) : null;
  return h('fieldset', Object.assign({
    className: cls,
    disabled: props.disabled || undefined,
    'data-invalid': invalid ? true : undefined,
    'aria-labelledby': legendId,
    'aria-readonly': readOnly || undefined,
    'aria-describedby': showHelper && helper ? helperId : undefined,
    onClickCapture: onClickCapture
  }, rest),
    h('legend', { className: 'cds--label', id: legendId }, props.legendText,
      props.decorator ? h('div', { className: 'cds--checkbox-group-inner--decorator' }, props.decorator) : null),
    props.children,
    h('div', { className: 'cds--checkbox-group__validation-msg' },
      !readOnly && invalid ? [
        h(Icon, { key: 'i', name: 'warning--filled', size: 16, className: 'cds--checkbox__invalid-icon' }),
        h('div', { key: 't', className: 'cds--form-requirement' }, props.invalidText)
      ] : null,
      showWarning ? [
        h(React.Fragment, { key: 'w' }, g4WarnAltIcon('cds--checkbox__invalid-icon cds--checkbox__invalid-icon--warning')),
        h('div', { key: 't', className: 'cds--form-requirement' }, props.warnText)
      ] : null),
    showHelper ? helper : null);
}
exports.CheckboxGroup = CheckboxGroup;

// Heading / Section -----------------------------------------------------------
// packages/react/src/components/Heading: each Section bumps the level; Heading renders h<level> (max 6).
var HeadingContext = React.createContext(1);
var Section = React.forwardRef(function Section(props, ref) {
  var parent = React.useContext(HeadingContext);
  var level = props.level != null ? props.level : parent + 1;
  var Base = props.as || 'section';
  return h(HeadingContext.Provider, { value: Math.max(1, Math.min(level, 6)) },
    h(Base, Object.assign({ ref: ref }, g4Omit(props, ['as', 'level']))));
});
exports.Section = Section;
var Heading = React.forwardRef(function Heading(props, ref) {
  var level = React.useContext(HeadingContext);
  return h('h' + level, Object.assign({ ref: ref }, props));
});
exports.Heading = Heading;

// Theme / GlobalTheme / useTheme --------------------------------------------------
// packages/react/src/components/Theme. Zones here set data-theme (tokens.css re-binds every color token
// under [data-theme]) plus Carbon's cds--<theme> class; .cds--white/g10/g90/g100 paint background + text.
var ThemeContext = React.createContext({ theme: 'white', isDark: false });
var GlobalTheme = React.forwardRef(function GlobalTheme(props, ref) {
  var theme = props.theme;
  var value = React.useMemo(function () { return { theme: theme, isDark: theme === 'g90' || theme === 'g100' }; }, [theme]);
  var child = React.isValidElement(props.children) ? React.cloneElement(props.children, { ref: ref }) : props.children;
  return h(ThemeContext.Provider, { value: value }, child);
});
exports.GlobalTheme = GlobalTheme;

var G4_THEMES = ['white', 'g10', 'g90', 'g100'];
function Theme(props) {
  var theme = G4_THEMES.indexOf(props.theme) > -1 ? props.theme : undefined;
  var Base = props.as || 'div';
  var rest = g4Omit(props, ['as', 'className', 'theme']);
  var value = React.useMemo(function () { return { theme: theme, isDark: theme === 'g90' || theme === 'g100' }; }, [theme]);
  var cls = cx(props.className, theme && 'cds--' + theme);
  var attrs = Object.assign({}, rest, { 'data-theme': theme });
  var L = exports.Layer;
  // A theme zone resets the layer stack to level one, as Carbon's Theme does with LayerContext.
  var el = L ? h(L, Object.assign({ as: Base, level: 1, className: cls }, attrs))
    : h(Base, Object.assign({}, attrs, { className: cx(cls, 'cds--layer-one') }));
  return h(ThemeContext.Provider, { value: value }, el);
}
exports.Theme = Theme;

function useTheme() { return React.useContext(ThemeContext); }
exports.useTheme = useTheme;

})(Carbon,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx);

/* ---- G5 ---- */
(function(exports,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx){
/* @components: DismissibleTag, OperationalTag, SelectableTag, Callout, DataTableSkeleton, SkeletonIcon, DefinitionTooltip */

// Shared: label overflow detection (Carbon isEllipsisActive: offsetWidth < scrollWidth).
function g5UseEllipsis(ref) {
  var st = useState(false); var on = st[0], setOn = st[1];
  useLayoutEffect(function () {
    var el = ref.current && ref.current.querySelector('.cds--tag__label');
    if (el) setOn(el.offsetWidth < el.scrollWidth);
  });
  return on;
}

function g5Rest(props, omit) {
  var out = {};
  Object.keys(props).forEach(function (k) { if (omit.indexOf(k) < 0) out[k] = props[k]; });
  return out;
}

function g5CustomIcon(props, size) {
  if (!props.renderIcon || size === 'sm') return null;
  return h('div', { className: 'cds--tag__custom-icon' },
    typeof props.renderIcon === 'string' ? h(Icon, { name: props.renderIcon, size: 16 }) : h(props.renderIcon, { size: 16 }));
}

// Wraps a truncated interactive tag in Carbon's icon tooltip showing the full text.
function g5MaybeTooltip(truncated, text, el, closeOnActivation) {
  var T = exports.Tooltip;
  if (!truncated || !T) return el;
  return h(T, { label: text, align: 'bottom', className: 'cds--icon-tooltip cds--tag-label-tooltip', leaveDelayMs: 0, closeOnActivation: closeOnActivation }, el);
}

// DismissibleTag -------------------------------------------------------------
var G5_OMIT = ['className', 'disabled', 'id', 'renderIcon', 'title', 'onClose', 'size', 'text', 'tagTitle', 'type', 'dismissTooltipAlignment', 'dismissTooltipLabel', 'onClick', 'children', 'decorator', 'onChange', 'selected', 'defaultSelected'];
function DismissibleTag(props) {
  var autoId = useId();
  var ref = useRef(null);
  var size = props.size || 'md';
  var truncated = g5UseEllipsis(ref);
  var title = props.title || 'Dismiss';
  var label = truncated ? (props.dismissTooltipLabel || 'Dismiss tag') : title;
  function handleClose(e) { if (props.onClose) { e.stopPropagation(); props.onClose(e); } }
  var tagId = props.id || ('tag-' + autoId), closeId = tagId + '-close', labelId = tagId + '-label';
  // Accessible name "<title> <text>" (e.g. "Dismiss us-east") so several close buttons in a filter bar are distinguishable;
  // aria-labelledby wins over the label the tooltip would set. The tooltip still shows only the short label.
  var btn = h('button', { type: 'button', id: closeId, className: 'cds--tag__close-icon', onClick: handleClose, disabled: props.disabled, 'aria-label': label, 'aria-labelledby': closeId + ' ' + labelId },
    h(Icon, { name: 'close', size: 16 }));
  var T = exports.Tooltip;
  var closeEl = T && !props.disabled
    ? h(T, { label: label, align: props.dismissTooltipAlignment || 'bottom', className: 'cds--icon-tooltip cds--tag-label-tooltip', leaveDelayMs: 0, closeOnActivation: true }, btn)
    : btn;
  return h('div', Object.assign(g5Rest(props, G5_OMIT), {
    ref: ref, id: tagId,
    className: cx('cds--tag', 'cds--tag--filter', props.className, props.disabled && 'cds--tag--disabled', 'cds--tag--' + size, 'cds--layout--size-' + size, 'cds--tag--' + (props.type || 'gray'))
  }),
    g5CustomIcon(props, size),
    h('div', { className: 'cds--interactive--tag-children' },
      h('span', { id: labelId, title: props.tagTitle || props.text, className: 'cds--tag__label' }, props.text),
      props.decorator ? h('div', { className: 'cds--tag__decorator' }, props.decorator) : null,
      closeEl));
}
exports.DismissibleTag = DismissibleTag;

// OperationalTag -------------------------------------------------------------
function OperationalTag(props) {
  var autoId = useId();
  var ref = useRef(null);
  var size = props.size || 'md';
  var truncated = g5UseEllipsis(ref);
  var el = h('button', Object.assign(g5Rest(props, G5_OMIT), {
    ref: ref, type: 'button', id: props.id || ('tag-' + autoId), disabled: props.disabled, onClick: props.onClick,
    className: cx('cds--tag', 'cds--tag--operational', props.className, props.disabled && 'cds--tag--disabled', 'cds--tag--' + size, 'cds--layout--size-' + size, 'cds--tag--' + (props.type || 'gray'))
  }),
    g5CustomIcon(props, size),
    h('span', { title: props.text, className: 'cds--tag__label' }, props.text));
  return g5MaybeTooltip(truncated, props.text, el, true);
}
exports.OperationalTag = OperationalTag;

// SelectableTag --------------------------------------------------------------
function SelectableTag(props) {
  var autoId = useId();
  var ref = useRef(null);
  var size = props.size || 'md';
  var truncated = g5UseEllipsis(ref);
  var st = useState(!!props.defaultSelected); var inner = st[0], setInner = st[1];
  var controlled = props.selected !== undefined;
  var sel = controlled ? !!props.selected : inner;
  function click(e) {
    if (!controlled) setInner(!sel);
    if (props.onChange) props.onChange(!sel);
    if (props.onClick) props.onClick(e);
  }
  var el = h('button', Object.assign(g5Rest(props, G5_OMIT), {
    ref: ref, type: 'button', id: props.id || ('tag-' + autoId), disabled: props.disabled, onClick: click, 'aria-pressed': sel,
    className: cx('cds--tag', 'cds--tag--selectable', props.className, sel && 'cds--tag--selectable-selected', props.disabled && 'cds--tag--disabled', 'cds--tag--' + size, 'cds--layout--size-' + size)
  }),
    g5CustomIcon(props, size),
    h('span', { title: props.text, className: 'cds--tag__label' }, props.text));
  return g5MaybeTooltip(truncated, props.text, el, false);
}
exports.SelectableTag = SelectableTag;

// Callout --------------------------------------------------------------------
// Carbon uses InformationFilled for info; the 32px source is drawn at 20px via CSS so its circle is not cropped.
var G5_CALLOUT_ICON = { error: ['error--filled', 20], success: ['checkmark--filled', 20], warning: ['warning--filled', 20], 'warning-alt': ['warning--alt--filled', 32], info: ['information--filled', 32], 'info-square': ['information--square--filled', 32] };
function Callout(props) {
  var kind = props.kind || 'info';
  var low = props.lowContrast !== false;
  var ic = G5_CALLOUT_ICON[kind] || G5_CALLOUT_ICON.info;
  var rest = g5Rest(props, ['actionButtonLabel', 'children', 'onActionButtonClick', 'title', 'titleId', 'subtitle', 'statusIconDescription', 'className', 'kind', 'lowContrast']);
  var B = exports.Button;
  return h('div', Object.assign(rest, {
    className: cx(props.className, 'cds--actionable-notification', low && 'cds--actionable-notification--low-contrast', 'cds--actionable-notification--' + kind, 'cds--actionable-notification--hide-close-button')
  }),
    h('div', { className: 'cds--actionable-notification__details' },
      h(Icon, { name: ic[0], size: ic[1], className: 'cds--inline-notification__icon', title: props.statusIconDescription || (kind + ' icon'), style: { width: 20, height: 20 } }),
      h('div', { className: 'cds--actionable-notification__text-wrapper' },
        props.title ? h('div', { id: props.titleId, className: 'cds--actionable-notification__title' }, props.title) : null,
        props.subtitle ? h('div', { className: 'cds--actionable-notification__subtitle' }, props.subtitle) : null,
        props.children)),
    h('div', { className: 'cds--actionable-notification__button-wrapper' },
      props.actionButtonLabel && B ? h(B, { kind: 'ghost', size: 'sm', className: 'cds--actionable-notification__action-button', onClick: props.onActionButtonClick, 'aria-describedby': props.titleId }, props.actionButtonLabel) : null));
}
exports.Callout = Callout;

// DataTableSkeleton ----------------------------------------------------------
function DataTableSkeleton(props) {
  var rowCount = props.rowCount === undefined ? 5 : props.rowCount;
  var columnCount = props.columnCount === undefined ? 5 : props.columnCount;
  var showHeader = props.showHeader !== false, showToolbar = props.showToolbar !== false;
  var headers = props.headers;
  var cols = []; for (var c = 0; c < columnCount; c++) cols.push(c);
  var rows = []; for (var r = 0; r < rowCount; r++) rows.push(r);
  var rest = g5Rest(props, ['headers', 'rowCount', 'columnCount', 'zebra', 'compact', 'className', 'showHeader', 'showToolbar', 'size']);
  return h('div', { className: 'cds--skeleton cds--data-table-container' },
    showHeader ? h('div', { className: 'cds--data-table-header' },
      h('div', { className: 'cds--data-table-header__title' }),
      h('div', { className: 'cds--data-table-header__description' })) : null,
    showToolbar ? h('section', { 'aria-label': 'data table toolbar', className: 'cds--table-toolbar' },
      h('div', { className: 'cds--toolbar-content' },
        h('span', { className: 'cds--skeleton cds--btn cds--btn--sm' }))) : null,
    h('table', Object.assign(rest, {
      className: cx(props.className, 'cds--skeleton', 'cds--data-table', props.zebra && 'cds--data-table--zebra', props.compact && 'cds--data-table--compact')
    }),
      h('thead', null, h('tr', null, cols.map(function (i) {
        return h('th', { key: i }, headers ? h('div', { className: 'cds--table-header-label' }, headers[i] ? headers[i].header : null) : h('span'));
      }))),
      h('tbody', null, rows.map(function (i) {
        return h('tr', { key: i }, cols.map(function (j) { return h('td', { key: j }, h('span')); }));
      }))));
}
exports.DataTableSkeleton = DataTableSkeleton;

// SkeletonIcon ---------------------------------------------------------------
function SkeletonIcon(props) {
  var rest = g5Rest(props, ['className']);
  return h('div', Object.assign(rest, { className: cx(props.className, 'cds--icon--skeleton') }));
}
exports.SkeletonIcon = SkeletonIcon;

// DefinitionTooltip ----------------------------------------------------------
function DefinitionTooltip(props) {
  var st = useState(!!props.defaultOpen); var isOpen = st[0], setOpen = st[1];
  var autoId = useId(); var tooltipId = props.id || ('definition-tooltip-' + autoId);
  var P = exports.Popover, PC = exports.PopoverContent;
  var rest = g5Rest(props, ['align', 'autoAlign', 'className', 'children', 'definition', 'defaultOpen', 'id', 'openOnHover', 'tooltipText', 'triggerClassName']);
  function onKeyDown(e) {
    if (isOpen && e.key === 'Escape') { e.stopPropagation(); setOpen(false); }
    if (props.onKeyDown) props.onKeyDown(e);
  }
  var trigger = h('button', Object.assign(rest, {
    className: cx('cds--definition-term', props.triggerClassName),
    'aria-controls': tooltipId, 'aria-describedby': tooltipId, 'aria-expanded': isOpen,
    onBlur: function () { setOpen(false); },
    onMouseDown: function (e) { if (e.button === 0) setOpen(!isOpen); },
    onKeyDown: onKeyDown, type: 'button'
  }), props.children);
  var content = props.tooltipText !== undefined ? props.tooltipText : props.definition;
  return h(P, {
    align: props.align || 'bottom', className: props.className, dropShadow: false, highContrast: true, open: isOpen,
    onMouseLeave: function () { setOpen(false); },
    onMouseEnter: function () { if (props.openOnHover) setOpen(true); },
    onFocus: function () { setOpen(true); }
  },
    trigger,
    h(PC, { className: 'cds--definition-tooltip', id: tooltipId }, content));
}
exports.DefinitionTooltip = DefinitionTooltip;

})(Carbon,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx);

/* ---- G6 ---- */
(function(exports,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx){
/* @components: PageHeader, PageHeaderBreadcrumbBar, PageHeaderContent, PageHeaderContentPageActions, PageHeaderContentText, PageHeaderHeroImage, PageHeaderTabBar, Dialog, DialogHeader, DialogControls, DialogCloseButton, DialogTitle, DialogSubtitle, DialogBody, DialogFooter */

function g6omit(props, keys) {
  var out = {};
  for (var k in props) { if (Object.prototype.hasOwnProperty.call(props, k) && keys.indexOf(k) < 0) out[k] = props[k]; }
  return out;
}
// renderIcon accepts an icon file stem ("bee"), a React element, or a component.
function g6icon(icon, size) {
  if (!icon) return null;
  if (typeof icon === 'string') return h(Icon, { name: icon, size: size });
  if (React.isValidElement(icon)) return icon;
  return h(icon, { size: size });
}
// Full-width 16/8/4 column wrapper, using the bundle's Grid/Column when present.
function g6Grid(children) {
  var G = exports.Grid, Col = exports.Column;
  if (G && Col) return h(G, null, h(Col, { lg: 16, md: 8, sm: 4 }, children));
  return h('div', { style: { paddingInline: '2rem' } }, children);
}

// ============================================================== PageHeader
function PageHeader(props) {
  var rest = g6omit(props, ['className', 'children']);
  return h('div', Object.assign({}, rest, { className: cx('cds--page-header', props.className) }), props.children);
}
PageHeader.displayName = 'PageHeader';

function PageHeaderBreadcrumbBar(props) {
  var border = props.border === undefined ? true : props.border;
  var rest = g6omit(props, ['border', 'className', 'children', 'renderIcon', 'contentActions', 'contentActionsFlush', 'pageActions', 'pageActionsFlush']);
  var cls = cx('cds--page-header__breadcrumb-bar', border && 'cds--page-header__breadcrumb-bar-border', props.pageActionsFlush && 'cds--page-header__breadcrumb__actions-flush', props.className);
  return h('div', Object.assign({}, rest, { className: cls }),
    g6Grid(h('div', { className: 'cds--page-header__breadcrumb-container' },
      h('div', { className: 'cds--page-header__breadcrumb-wrapper' },
        props.renderIcon ? h('div', { className: 'cds--page-header__breadcrumb__icon' }, g6icon(props.renderIcon, 16)) : null,
        props.children),
      h('div', { className: 'cds--page-header__breadcrumb__actions' },
        h('div', { className: cx(!props.contentActionsFlush && 'cds--page-header__breadcrumb__content-actions') }, props.contentActions),
        props.pageActions))));
}
PageHeaderBreadcrumbBar.displayName = 'PageHeaderBreadcrumbBar';

function PageHeaderContent(props) {
  var rest = g6omit(props, ['className', 'children', 'title', 'renderIcon', 'contextualActions', 'pageActions']);
  var titleRef = useRef(null);
  var st = useState(false); var clipped = st[0], setClipped = st[1];
  useLayoutEffect(function () {
    function check() { var el = titleRef.current; if (el) setClipped(el.offsetHeight < el.scrollHeight); }
    check();
    window.addEventListener('resize', check);
    return function () { window.removeEventListener('resize', check); };
  }, [props.title, !!props.contextualActions]);
  return h('div', Object.assign({}, rest, { className: cx('cds--page-header__content', props.className) }),
    g6Grid([
      h('div', { key: 'tw', className: 'cds--page-header__content__title-wrapper' },
        h('div', { className: 'cds--page-header__content__start' },
          h('div', { className: 'cds--page-header__content__title-container' },
            props.renderIcon ? h('div', { className: 'cds--page-header__content__icon' }, g6icon(props.renderIcon, 32)) : null,
            h('h4', { ref: titleRef, className: 'cds--page-header__content__title', title: clipped ? props.title : undefined }, props.title)),
          props.contextualActions ? h('div', { className: 'cds--page-header__content__contextual-actions' }, props.contextualActions) : null),
        props.pageActions),
      h(React.Fragment, { key: 'c' }, props.children)
    ]));
}
PageHeaderContent.displayName = 'PageHeaderContent';

function PageHeaderContentPageActions(props) {
  var rest = g6omit(props, ['className', 'children', 'menuButtonLabel', 'actions']);
  var actions = props.actions;
  var body = Array.isArray(actions)
    ? actions.map(function (a, i) {
        var el = a.body;
        if (React.isValidElement(el) && a.onClick) el = React.cloneElement(el, { onClick: a.onClick });
        return h('div', { key: a.id || i }, el);
      })
    : actions;
  return h('div', Object.assign({}, rest, { className: cx('cds--page-header__content__page-actions', props.className) }), body);
}
PageHeaderContentPageActions.displayName = 'PageHeaderContentPageActions';

function PageHeaderContentText(props) {
  var rest = g6omit(props, ['className', 'children', 'subtitle']);
  return h('div', Object.assign({}, rest, { className: cx('cds--page-header__content__body', props.className) }),
    props.subtitle ? h('h3', { className: 'cds--page-header__content__subtitle' }, props.subtitle) : null,
    props.children);
}
PageHeaderContentText.displayName = 'PageHeaderContentText';

function PageHeaderHeroImage(props) {
  var rest = g6omit(props, ['className', 'children']);
  var mq = '(min-width: 66rem)';
  var st = useState(function () { return typeof window !== 'undefined' && window.matchMedia ? window.matchMedia(mq).matches : true; });
  var isLg = st[0], setIsLg = st[1];
  useEffect(function () {
    if (!window.matchMedia) return;
    var m = window.matchMedia(mq);
    function on() { setIsLg(m.matches); }
    on();
    if (m.addEventListener) m.addEventListener('change', on); else m.addListener(on);
    return function () { if (m.removeEventListener) m.removeEventListener('change', on); else m.removeListener(on); };
  }, []);
  var ratio = isLg ? '2x1' : '3x2';
  var cls = cx('cds--page-header__hero-image', props.className);
  var AR = exports.AspectRatio;
  if (AR) return h(AR, Object.assign({}, rest, { className: cls, ratio: ratio }), props.children);
  return h('div', Object.assign({}, rest, { className: cx(cls, 'cds--aspect-ratio', 'cds--aspect-ratio--' + ratio) }), props.children);
}
PageHeaderHeroImage.displayName = 'PageHeaderHeroImage';

// Tab strip with the cds--tabs markup, controlled or uncontrolled, so the page can swap its content on change.
function G6TabList(props) {
  var tabs = props.tabs;
  var controlled = props.selectedIndex !== undefined;
  var st = useState(props.defaultSelectedIndex || 0);
  var idx = controlled ? props.selectedIndex : st[0];
  var autoId = useId();
  function select(i) {
    if (tabs[i] && tabs[i].disabled) return;
    if (!controlled) st[1](i);
    if (props.onChange) props.onChange({ selectedIndex: i });
  }
  function onKey(e) {
    var n = tabs.length, next = -1, i, k;
    if (e.key === 'ArrowRight' || e.key === 'ArrowLeft') {
      var dir = e.key === 'ArrowRight' ? 1 : -1; k = idx;
      for (i = 0; i < n; i++) { k = (k + dir + n) % n; if (!tabs[k].disabled) { next = k; break; } }
    } else if (e.key === 'Home') {
      for (i = 0; i < n; i++) { if (!tabs[i].disabled) { next = i; break; } }
    } else if (e.key === 'End') {
      for (i = n - 1; i >= 0; i--) { if (!tabs[i].disabled) { next = i; break; } }
    }
    if (next < 0) return;
    e.preventDefault(); select(next);
    var btns = e.currentTarget.querySelectorAll('[role=tab]'); if (btns[next]) btns[next].focus();
  }
  return h('div', { className: 'cds--tabs' },
    h('div', { className: 'cds--tabs__nav', role: 'tablist', 'aria-label': props.ariaLabel || 'Page tabs', onKeyDown: onKey },
      tabs.map(function (t, i) {
        var sel = i === idx;
        return h('div', { key: t.id || i, className: cx('cds--tabs__nav-item', sel && 'cds--tabs__nav-item--selected', t.disabled && 'cds--tabs__nav-item--disabled') },
          h('button', { type: 'button', role: 'tab', id: t.id || (autoId + '-tab-' + i), 'aria-selected': sel, 'aria-controls': t.panelId, tabIndex: sel ? 0 : -1, disabled: t.disabled, className: 'cds--tabs__nav-link', onClick: function () { select(i); } }, t.label));
      })));
}

function PageHeaderTabBar(props) {
  var rest = g6omit(props, ['className', 'children', 'tags', 'tabs', 'selectedIndex', 'defaultSelectedIndex', 'onChange', 'ariaLabel']);
  var tags = props.tags || [];
  var Tag = exports.Tag;
  var strip = props.tabs
    ? h(G6TabList, { tabs: props.tabs, selectedIndex: props.selectedIndex, defaultSelectedIndex: props.defaultSelectedIndex, onChange: props.onChange, ariaLabel: props.ariaLabel })
    : props.children;
  var inner = tags.length
    ? h('div', { className: 'cds--page-header__tab-bar--tablist' },
        strip,
        h('div', { className: 'cds--page-header__tags' },
          tags.map(function (t, i) {
            return h(Tag, { key: t.id || i, type: t.type, size: t.size, className: 'cds--page-header__tag-item' }, t.text);
          })))
    : strip;
  return h('div', Object.assign({}, rest, { className: cx('cds--page-header__tab-bar', props.className) }), g6Grid(inner));
}
PageHeaderTabBar.displayName = 'PageHeaderTabBar';

exports.PageHeader = PageHeader;
exports.PageHeaderBreadcrumbBar = PageHeaderBreadcrumbBar;
exports.PageHeaderContent = PageHeaderContent;
exports.PageHeaderContentPageActions = PageHeaderContentPageActions;
exports.PageHeaderContentText = PageHeaderContentText;
exports.PageHeaderHeroImage = PageHeaderHeroImage;
exports.PageHeaderTabBar = PageHeaderTabBar;

// ============================================================== Dialog
var DialogContext = React.createContext({});

function Dialog(props) {
  var open = !!props.open, modal = !!props.modal;
  var onRequestClose = props.onRequestClose || function () {};
  var ref = useRef(null);
  var dialogId = useId();
  var titleId = 'cds--dialog-header__heading--' + dialogId;
  var subtitleId = 'cds--dialog-header__label--' + dialogId;
  var rest = g6omit(props, ['children', 'className', 'focusAfterCloseRef', 'modal', 'onCancel', 'onClick', 'onClose', 'onRequestClose', 'open', 'role', 'ariaLabel', 'ariaLabelledBy', 'ariaDescribedBy', 'inline', 'size', 'onKeyDown']);

  useEffect(function () {
    var el = ref.current; if (!el) return;
    if (open) {
      if (el.open) {
        var isModal = false; try { isModal = el.matches(':modal'); } catch (err) { isModal = false; }
        if (isModal === modal) return;
        el.close();
      }
      if (modal) el.showModal(); else el.show();
    } else if (el.open) {
      el.close();
    }
  }, [modal, open]);

  // Focus focusAfterCloseRef only when the dialog goes from open to closed (not on a first render that starts closed).
  var wasOpen = useRef(open);
  useEffect(function () {
    var r = props.focusAfterCloseRef;
    var closing = wasOpen.current && !open;
    wasOpen.current = open;
    if (closing && r) {
      var t = setTimeout(function () { if (r.current) r.current.focus(); });
      return function () { clearTimeout(t); };
    }
  }, [open, props.focusAfterCloseRef]);

  function handleClick(e) {
    if (open && modal && e.target === ref.current) onRequestClose(e);
    if (props.onClick) props.onClick(e);
  }
  // Escape on a modal dialog: keep the open state owned by the parent instead of letting the browser close it.
  function handleCancel(e) {
    if (props.onCancel) props.onCancel(e);
    if (!e.defaultPrevented) { e.preventDefault(); onRequestClose(e); }
  }

  // Browsers fire `cancel` on Escape only for modal dialogs; give non-modal ones the same Escape behavior.
  function handleKeyDown(e) {
    if (props.onKeyDown) props.onKeyDown(e);
    if (open && !modal && e.key === 'Escape' && !e.defaultPrevented) {
      if (props.onCancel) props.onCancel(e);
      if (!e.defaultPrevented) { e.preventDefault(); onRequestClose(e); }
    }
  }

  var ctx = { dialogId: dialogId, titleId: titleId, subtitleId: subtitleId, isOpen: open };
  return h(DialogContext.Provider, { value: ctx },
    h('dialog', Object.assign({}, rest, {
      ref: ref,
      className: cx('cds--dialog', props.size && props.size !== 'md' && 'cds--dialog--' + props.size, modal && 'cds--dialog--modal', props.inline && 'cds--dialog--inline', props.className),
      onCancel: handleCancel, onClick: handleClick, onClose: props.onClose, onKeyDown: handleKeyDown,
      role: props.role || undefined,
      'aria-label': props.ariaLabel || props['aria-label'],
      'aria-labelledby': (props.ariaLabel || props['aria-label']) ? undefined : (props.ariaLabelledBy || props['aria-labelledby'] || titleId),
      'aria-describedby': props.ariaDescribedBy || props['aria-describedby']
    }), h('div', { className: 'cds--dialog-container' }, props.children)));
}
Dialog.displayName = 'Dialog';

function DialogHeader(props) {
  var rest = g6omit(props, ['className', 'children']);
  return h('div', Object.assign({}, rest, { className: cx('cds--dialog__header', props.className) }), props.children);
}
DialogHeader.displayName = 'DialogHeader';

function DialogControls(props) {
  var rest = g6omit(props, ['className', 'children']);
  return h('div', Object.assign({}, rest, { className: cx('cds--dialog__header-controls', props.className) }), props.children);
}
DialogControls.displayName = 'DialogControls';

function DialogCloseButton(props) {
  var IB = exports.IconButton;
  var label = props.label || 'Close';
  var glyph = h(Icon, { name: 'close', size: 20, className: 'cds--icon__close' });
  if (IB) return h(IB, { kind: 'ghost', size: 'lg', className: 'cds--dialog__close', label: label, 'aria-label': label, align: 'left', onClick: props.onClick }, glyph);
  return h('button', { type: 'button', className: 'cds--btn cds--btn--ghost cds--btn--lg cds--btn--icon-only cds--dialog__close', 'aria-label': label, title: label, onClick: props.onClick }, glyph);
}
DialogCloseButton.displayName = 'DialogCloseButton';

function DialogTitle(props) {
  var ctx = React.useContext(DialogContext);
  var rest = g6omit(props, ['className', 'children', 'id']);
  return h('h2', Object.assign({}, rest, { id: props.id || ctx.titleId, className: cx('cds--dialog-header__heading', props.className) }), props.children);
}
DialogTitle.displayName = 'DialogTitle';

function DialogSubtitle(props) {
  var ctx = React.useContext(DialogContext);
  var rest = g6omit(props, ['className', 'children', 'id']);
  return h('h2', Object.assign({}, rest, { id: props.id || ctx.subtitleId, className: cx('cds--dialog-header__label', props.className) }), props.children);
}
DialogSubtitle.displayName = 'DialogSubtitle';

function DialogBody(props) {
  var rest = g6omit(props, ['className', 'children', 'hasScrollingContent']);
  var ref = useRef(null);
  var st = useState(false); var scrollable = st[0], setScrollable = st[1];
  var id = useId();
  var ctx = React.useContext(DialogContext);
  // Re-measure when the dialog opens (a closed <dialog> is display:none, so the first layout pass reads 0) and on resize.
  useLayoutEffect(function () {
    var t = null, raf = null, ro = null;
    function check() { var el = ref.current; if (el && el.clientHeight > 0) setScrollable(el.scrollHeight > el.clientHeight + 1); }
    function deb() { clearTimeout(t); t = setTimeout(check, 200); }
    check();
    raf = requestAnimationFrame(check);
    window.addEventListener('resize', deb);
    if (typeof ResizeObserver !== 'undefined' && ref.current) { ro = new ResizeObserver(function () { check(); }); ro.observe(ref.current); }
    return function () { clearTimeout(t); cancelAnimationFrame(raf); window.removeEventListener('resize', deb); if (ro) ro.disconnect(); };
  }, [ctx.isOpen]);
  var scroll = props.hasScrollingContent || scrollable;
  // A scrolling body is a focusable region; name it after the dialog title.
  var p = Object.assign({ id: 'cds--dialog-body--' + id }, scroll ? { tabIndex: 0, role: 'region', 'aria-labelledby': ctx.titleId } : {}, rest, {
    className: cx('cds--dialog-content', scroll && 'cds--dialog-scroll-content', props.className)
  });
  var L = exports.Layer;
  if (L) {
    // Layer does not forward refs, so measure through a wrapper callback on the element it renders.
    return h(L, Object.assign({}, p, { ref: undefined, as: G6BodyDiv, innerRef: ref }), props.children);
  }
  return h('div', Object.assign({}, p, { ref: ref }), props.children);
}
function G6BodyDiv(props) {
  var rest = g6omit(props, ['innerRef']);
  return h('div', Object.assign({}, rest, { ref: props.innerRef }));
}
DialogBody.displayName = 'DialogBody';

function DialogFooter(props) {
  var ctx = React.useContext(DialogContext);
  var Button = exports.Button, BS = exports.ButtonSet;
  var onRequestClose = props.onRequestClose || function () {};
  var loadingStatus = props.loadingStatus || 'inactive';
  var loading = loadingStatus !== 'inactive';
  var danger = !!props.danger;
  var secRef = useRef(null);
  var rest = g6omit(props, ['children', 'className', 'onRequestClose', 'onSecondarySubmit', 'onRequestSubmit', 'primaryButtonText', 'primaryButtonDisabled', 'secondaryButtonText', 'secondaryButtons', 'danger', 'loadingStatus', 'loadingDescription', 'loadingIconDescription', 'onLoadingSuccess']);
  var secs = props.secondaryButtons;
  var cls = cx('cds--dialog-footer', props.className, Array.isArray(secs) && secs.length === 2 && 'cds--dialog-footer--three-button');
  var Wrap = function (p, kids) { return BS ? h(BS, p, kids) : h('div', Object.assign({}, p, { className: cx(p.className, 'cds--btn-set') }), kids); };

  useEffect(function () {
    if (!danger || !ctx.isOpen) return;
    var f = requestAnimationFrame(function () {
      var root = secRef.current; var b = root && root.querySelector('.cds--btn--secondary');
      if (b) b.focus();
    });
    return function () { cancelAnimationFrame(f); };
  }, [danger, ctx.isOpen]);

  if (props.children) return Wrap(Object.assign({}, rest, { className: cls }), props.children);

  var primaryText = props.primaryButtonText === undefined ? 'Save' : props.primaryButtonText;
  var secondaryText = props.secondaryButtonText === undefined ? 'Cancel' : props.secondaryButtonText;
  var onSecondary = props.onSecondarySubmit || onRequestClose;
  var kids = [];
  if (Array.isArray(secs) && secs.length <= 2) {
    secs.forEach(function (s, i) { kids.push(h(Button, { key: 's' + i, kind: 'secondary', onClick: s.onClick }, s.buttonText)); });
  } else if (secondaryText) {
    kids.push(h(Button, { key: 's', kind: 'secondary', disabled: loading, onClick: onSecondary }, secondaryText));
  }
  var IL = exports.InlineLoading;
  var primaryContent = !loading ? primaryText
    : (IL ? h(IL, { status: loadingStatus, description: props.loadingDescription, iconDescription: props.loadingIconDescription, className: 'cds--inline-loading--btn', onSuccess: props.onLoadingSuccess }) : props.loadingDescription);
  kids.push(h(Button, { key: 'p', kind: danger ? 'danger' : 'primary', className: loading ? 'cds--btn--loading' : undefined, disabled: loading || !!props.primaryButtonDisabled, onClick: props.onRequestSubmit }, primaryContent));
  return h('div', { ref: secRef, style: { display: 'contents' } }, Wrap(Object.assign({}, rest, { className: cls, 'aria-busy': loading }), kids));
}
DialogFooter.displayName = 'DialogFooter';

exports.Dialog = Dialog;
exports.DialogHeader = DialogHeader;
exports.DialogControls = DialogControls;
exports.DialogCloseButton = DialogCloseButton;
exports.DialogTitle = DialogTitle;
exports.DialogSubtitle = DialogSubtitle;
exports.DialogBody = DialogBody;
exports.DialogFooter = DialogFooter;

})(Carbon,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx);

/* ---- G7 ---- */
(function(exports,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx){
/* @components: HeaderMenu, HeaderPanel, Switcher, SwitcherItem, SwitcherDivider, HeaderSideNavItems, SideNavHeader, SideNavFooter */

// ---------------------------------------------------------------- UI Shell additions (header menu, panel, switcher)
function g7omit(props, keys) {
  var rest = {};
  for (var k in props) { if (keys.indexOf(k) < 0) rest[k] = props[k]; }
  return rest;
}

function g7hasActiveDescendant(children) {
  return React.Children.toArray(children).some(function (c) {
    if (!React.isValidElement(c)) return false;
    var p = c.props;
    return !!(p.isActive || p.isCurrentPage || (Array.isArray(p.children) && g7hasActiveDescendant(p.children)));
  });
}

// HeaderMenu: a header nav entry that opens a sub-menu of HeaderMenuItem links below it.
function HeaderMenu(props) {
  var controlled = props.expanded !== undefined;
  var st = useState(!!props.defaultExpanded);
  var expanded = controlled ? !!props.expanded : st[0];
  var liRef = useRef(null);
  var btnRef = useRef(null);
  var menuRef = useRef(null);

  function setExpanded(v) {
    if (!controlled) st[1](v);
    if (props.onToggle) props.onToggle(v);
  }
  function items() {
    return menuRef.current ? Array.prototype.slice.call(menuRef.current.querySelectorAll('a.cds--header__menu-item, button.cds--header__menu-item')) : [];
  }
  function focusItem(i) {
    var list = items();
    if (!list.length) return;
    var n = list.length;
    list[((i % n) + n) % n].focus();
  }
  function setFocusRef(node) {
    btnRef.current = node;
    var r = props.focusRef;
    if (typeof r === 'function') r(node); else if (r && typeof r === 'object') r.current = node;
  }

  function onLiClick(e) {
    if (props.onClick) props.onClick(e);
    if (!menuRef.current || !(e.target instanceof Node) || !menuRef.current.contains(e.target)) e.preventDefault();
    else return; // clicks on sub-menu links follow the link
    setExpanded(!expanded);
  }
  function onTitleKeyDown(e) {
    if (e.key === 'Enter' || e.key === ' ' || e.key === 'Spacebar') {
      e.stopPropagation(); e.preventDefault();
      setExpanded(!expanded);
    } else if (e.key === 'ArrowDown') {
      e.preventDefault(); e.stopPropagation();
      if (!expanded) setExpanded(true);
      setTimeout(function () { focusItem(0); }, 0);
    } else if (e.key === 'ArrowUp' && expanded) {
      e.preventDefault(); e.stopPropagation();
      focusItem(-1);
    }
  }
  function onLiKeyDown(e) {
    if (props.onKeyDown) props.onKeyDown(e);
    if (!expanded) return;
    if (e.key === 'Escape') {
      e.stopPropagation(); e.preventDefault();
      setExpanded(false);
      if (btnRef.current) btnRef.current.focus();
      return;
    }
    var list = items();
    var idx = list.indexOf(document.activeElement);
    if (idx < 0) return;
    if (e.key === 'ArrowDown') { e.preventDefault(); focusItem(idx + 1); }
    else if (e.key === 'ArrowUp') { e.preventDefault(); focusItem(idx - 1); }
    else if (e.key === 'Home') { e.preventDefault(); focusItem(0); }
    else if (e.key === 'End') { e.preventDefault(); focusItem(list.length - 1); }
  }
  function onLiBlur(e) {
    if (props.onBlur) props.onBlur(e);
    if (liRef.current && e.relatedTarget && liRef.current.contains(e.relatedTarget)) return;
    if (expanded) setExpanded(false);
  }

  var active = props.isActive ? props.isActive : props.isCurrentPage;
  var label = { 'aria-label': props['aria-label'], 'aria-labelledby': props['aria-labelledby'] };
  var rest = g7omit(props, ['isActive', 'isCurrentPage', 'aria-label', 'aria-labelledby', 'className', 'children', 'renderMenuContent', 'menuLinkName', 'focusRef', 'onBlur', 'onClick', 'onKeyDown', 'expanded', 'defaultExpanded', 'onToggle', 'tabIndex']);
  var MenuContent = props.renderMenuContent;
  return h('li', Object.assign({}, rest, {
    ref: liRef,
    className: cx('cds--header__submenu', props.className),
    onKeyDown: onLiKeyDown, onClick: onLiClick, onBlur: onLiBlur
  }),
    h('a', Object.assign({
      'aria-haspopup': 'menu',
      'aria-expanded': expanded,
      className: cx('cds--header__menu-item', 'cds--header__menu-title', (active || (g7hasActiveDescendant(props.children) && !expanded)) && 'cds--header__menu-item--current'),
      href: '#',
      onKeyDown: onTitleKeyDown,
      ref: setFocusRef,
      tabIndex: props.tabIndex === undefined ? 0 : props.tabIndex
    }, label),
      props.menuLinkName,
      MenuContent ? h(MenuContent) : h(Icon, { name: 'chevron--down', size: 16, className: 'cds--header__menu-arrow' })),
    h('ul', Object.assign({}, label, { ref: menuRef, className: 'cds--header__menu' }), props.children));
}
HeaderMenu.displayName = 'HeaderMenu';

// HeaderPanel: right-hand 256px panel below the header.
function HeaderPanel(props) {
  var controlled = useRef(props.expanded !== undefined).current;
  var st = useState(props.expanded !== undefined ? props.expanded : !!props.defaultExpanded);
  var expanded = controlled ? !!props.expanded : st[0];
  var addFocus = props.addFocusListeners === undefined ? true : props.addFocusListeners;
  var ref = useRef(null);
  var lastClicked = useRef(null);
  var onFocusCb = props.onHeaderPanelFocus;

  function close() {
    if (!controlled) st[1](false);
    if (onFocusCb) onFocusCb();
  }
  function returnFocus() {
    var href = props.href;
    if (!href) return;
    if (href.charAt(0) === '#') {
      var el = document.getElementById(href.slice(1));
      if (el && el.focus) { el.focus(); return; }
    }
    window.location.href = href;
  }
  var handlers = {};
  if (addFocus) {
    handlers.onBlur = function (e) {
      var lc = lastClicked.current;
      if (!e.currentTarget.contains(e.relatedTarget) && !(lc && lc.classList && lc.classList.contains('cds--switcher__item-link'))) {
        if (!controlled) st[1](false);
        lastClicked.current = null;
        if (expanded && onFocusCb) onFocusCb();
      }
    };
    handlers.onKeyDown = function (e) {
      if (e.key === 'Escape') { close(); returnFocus(); }
    };
  }
  useEffect(function () {
    function onDocClick(e) {
      var t = e.target;
      if (!(t instanceof HTMLElement)) return;
      lastClicked.current = t;
      var kids = React.Children.toArray(props.children);
      var isSwitcher = kids.length === 1 && React.isValidElement(kids[0]) && kids[0].type === Switcher;
      if (isSwitcher && expanded && !t.closest('.cds--header-panel--expanded') && !t.closest('.cds--header__action')) close();
    }
    window.addEventListener('click', onDocClick);
    return function () { window.removeEventListener('click', onDocClick); };
  });
  var rest = g7omit(props, ['children', 'className', 'expanded', 'defaultExpanded', 'addFocusListeners', 'onHeaderPanelFocus', 'href']);
  // A labelled panel becomes a region landmark so its aria-label is announced (a generic div's label is not).
  var named = (props['aria-label'] || props['aria-labelledby']) && !props.role;
  return h('div', Object.assign(named ? { role: 'region' } : {}, rest, handlers, {
    ref: ref,
    className: cx('cds--header-panel', expanded && 'cds--header-panel--expanded', props.className)
  }), props.children);
}
HeaderPanel.displayName = 'HeaderPanel';

// Switcher: list of product links, usually inside a HeaderPanel.
function Switcher(props) {
  var ref = useRef(null);
  var kids = React.Children.toArray(props.children);
  function focusNext(o) {
    var enabled = [];
    kids.forEach(function (c, i) { if (React.isValidElement(c) && c.type === SwitcherItem) enabled.push(i); });
    if (!enabled.length) return;
    var pos = enabled.indexOf(o.currentIndex);
    var next = enabled[(pos + o.direction + enabled.length) % enabled.length];
    var li = ref.current && ref.current.children[next];
    var a = li && li.children[0];
    if (a) a.focus();
  }
  var rest = g7omit(props, ['children', 'className', 'expanded']);
  return h('ul', Object.assign({}, rest, { ref: ref, className: cx('cds--switcher', props.className) }),
    kids.map(function (c, i) {
      if (React.isValidElement(c) && c.type === SwitcherItem) {
        return React.cloneElement(c, { key: i, index: i, expanded: props.expanded, handleSwitcherItemFocus: focusNext });
      }
      if (React.isValidElement(c) && c.type === SwitcherDivider) return React.cloneElement(c, { key: i });
      return c;
    }));
}
Switcher.displayName = 'Switcher';

function SwitcherItem(props) {
  var tabIndex = props.tabIndex !== undefined ? props.tabIndex : (props.expanded ? 0 : -1);
  function onKeyDown(e) {
    if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
      e.preventDefault();
      if (props.handleSwitcherItemFocus) props.handleSwitcherItemFocus({ currentIndex: props.index === undefined ? -1 : props.index, direction: e.key === 'ArrowDown' ? 1 : -1 });
    }
    if (props.onKeyDown) props.onKeyDown(e);
  }
  var rest = g7omit(props, ['children', 'className', 'isSelected', 'expanded', 'tabIndex', 'index', 'handleSwitcherItemFocus', 'onKeyDown', 'element']);
  var El = props.element || 'a';
  return h('li', { className: cx('cds--switcher__item', props.className) },
    h(El, Object.assign({}, rest, {
      className: cx('cds--switcher__item-link', props.isSelected && 'cds--switcher__item-link--selected'),
      'aria-current': props.isSelected ? 'page' : rest['aria-current'],
      tabIndex: tabIndex,
      onKeyDown: onKeyDown
    }), props.children));
}
SwitcherItem.displayName = 'SwitcherItem';

function SwitcherDivider(props) {
  var rest = g7omit(props, ['className']);
  return h('hr', Object.assign({}, rest, { className: cx('cds--switcher__item--divider', props.className) }));
}
SwitcherDivider.displayName = 'SwitcherDivider';

// ---------------------------------------------------------------- Side nav additions
function HeaderSideNavItems(props) {
  return h('ul', { className: cx('cds--side-nav__header-navigation', props.hasDivider && 'cds--side-nav__header-divider', props.className) }, props.children);
}
HeaderSideNavItems.displayName = 'HeaderSideNavItems';

function g7navIcon(icon, size) {
  var node = typeof icon === 'string' ? h(Icon, { name: icon, size: size || 32 }) : (typeof icon === 'function' ? h(icon) : icon);
  return h('div', { className: 'cds--side-nav__icon' }, node);
}

function SideNavHeader(props) {
  return h('header', { className: cx('cds--side-nav__header', props.className) },
    props.renderIcon ? g7navIcon(props.renderIcon) : null,
    props.children);
}
SideNavHeader.displayName = 'SideNavHeader';

function SideNavFooter(props) {
  var text = props.assistiveText || 'Toggle opening or closing the side navigation';
  return h('footer', { className: cx('cds--side-nav__footer', props.className) },
    h('button', { className: 'cds--side-nav__toggle', type: 'button', title: text, 'aria-expanded': !!props.expanded, onClick: function (e) { if (props.onToggle) props.onToggle(e); } },
      h('div', { className: 'cds--side-nav__icon' }, props.expanded ? h(Icon, { name: 'close', size: 20 }) : h(Icon, { name: 'chevron--right', size: 20 })),
      h('span', { className: 'cds--assistive-text' }, text)));
}
SideNavFooter.displayName = 'SideNavFooter';

exports.HeaderMenu = HeaderMenu; exports.HeaderPanel = HeaderPanel;
exports.Switcher = Switcher; exports.SwitcherItem = SwitcherItem; exports.SwitcherDivider = SwitcherDivider;
exports.HeaderSideNavItems = HeaderSideNavItems; exports.SideNavHeader = SideNavHeader; exports.SideNavFooter = SideNavFooter;

})(Carbon,React,h,useState,useId,useRef,useEffect,useLayoutEffect,Icon,cx);

})();
