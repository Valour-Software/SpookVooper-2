(() => {
    var __webpack_modules__ = {
        634: (__unused_webpack_module, __unused_webpack___webpack_exports__, __webpack_require__) => {
            "use strict";
            eval("/* harmony import */ var _src_panzoom__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(36);\n\nconsole.log('This is a demo version of Panzoom for testing.');\nconsole.log('It exposes a global (window.Panzoom) and should not be used in production.');\nwindow.Panzoom = _src_panzoom__WEBPACK_IMPORTED_MODULE_0__/* .default */ .Z;\n//# sourceURL=[module]\n//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiNjM0LmpzIiwibWFwcGluZ3MiOiI7QUFBb0M7QUFFcEMsT0FBTyxDQUFDLEdBQUcsQ0FBQyxnREFBZ0QsQ0FBQztBQUM3RCxPQUFPLENBQUMsR0FBRyxDQUFDLDRFQUE0RSxDQUFDO0FBT3pGLE1BQU0sQ0FBQyxPQUFPLEdBQUcsMERBQU8iLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly9AcGFuem9vbS9wYW56b29tLy4vZGVtby9nbG9iYWwtcGFuem9vbS50cz83YjgwIl0sInNvdXJjZXNDb250ZW50IjpbImltcG9ydCBQYW56b29tIGZyb20gJy4uL3NyYy9wYW56b29tJ1xuXG5jb25zb2xlLmxvZygnVGhpcyBpcyBhIGRlbW8gdmVyc2lvbiBvZiBQYW56b29tIGZvciB0ZXN0aW5nLicpXG5jb25zb2xlLmxvZygnSXQgZXhwb3NlcyBhIGdsb2JhbCAod2luZG93LlBhbnpvb20pIGFuZCBzaG91bGQgbm90IGJlIHVzZWQgaW4gcHJvZHVjdGlvbi4nKVxuXG5kZWNsYXJlIGdsb2JhbCB7XG4gIGludGVyZmFjZSBXaW5kb3cge1xuICAgIFBhbnpvb206IHR5cGVvZiBQYW56b29tXG4gIH1cbn1cbndpbmRvdy5QYW56b29tID0gUGFuem9vbVxuIl0sIm5hbWVzIjpbXSwic291cmNlUm9vdCI6IiJ9\n//# sourceURL=webpack-internal:///634\n")
        }
        ,
        36: (__unused_webpack_module, __webpack_exports__, __webpack_require__) => {
            "use strict";
            eval("\n// EXPORTS\n__webpack_require__.d(__webpack_exports__, {\n  \"Z\": () => (/* binding */ panzoom)\n});\n\n// EXTERNAL MODULE: ./src/polyfills.js\nvar polyfills = __webpack_require__(252);\n;// CONCATENATED MODULE: ./src/pointers.ts\n/**\n * Utilites for working with multiple pointer events\n */\nfunction findEventIndex(pointers, event) {\n    var i = pointers.length;\n    while (i--) {\n        if (pointers[i].pointerId === event.pointerId) {\n            return i;\n        }\n    }\n    return -1;\n}\nfunction addPointer(pointers, event) {\n    var i;\n    // Add touches if applicable\n    if (event.touches) {\n        i = 0;\n        for (var _i = 0, _a = event.touches; _i < _a.length; _i++) {\n            var touch = _a[_i];\n            touch.pointerId = i++;\n            addPointer(pointers, touch);\n        }\n        return;\n    }\n    i = findEventIndex(pointers, event);\n    // Update if already present\n    if (i > -1) {\n        pointers.splice(i, 1);\n    }\n    pointers.push(event);\n}\nfunction removePointer(pointers, event) {\n    // Add touches if applicable\n    if (event.touches) {\n        // Remove all touches\n        while (pointers.length) {\n            pointers.pop();\n        }\n        return;\n    }\n    var i = findEventIndex(pointers, event);\n    if (i > -1) {\n        pointers.splice(i, 1);\n    }\n}\n/**\n * Calculates a center point between\n * the given pointer events, for panning\n * with multiple pointers.\n */\nfunction getMiddle(pointers) {\n    // Copy to avoid changing by reference\n    pointers = pointers.slice(0);\n    var event1 = pointers.pop();\n    var event2;\n    while ((event2 = pointers.pop())) {\n        event1 = {\n            clientX: (event2.clientX - event1.clientX) / 2 + event1.clientX,\n            clientY: (event2.clientY - event1.clientY) / 2 + event1.clientY\n        };\n    }\n    return event1;\n}\n/**\n * Calculates the distance between two points\n * for pinch zooming.\n * Limits to the first 2\n */\nfunction getDistance(pointers) {\n    if (pointers.length < 2) {\n        return 0;\n    }\n    var event1 = pointers[0];\n    var event2 = pointers[1];\n    return Math.sqrt(Math.pow(Math.abs(event2.clientX - event1.clientX), 2) +\n        Math.pow(Math.abs(event2.clientY - event1.clientY), 2));\n}\n\n;// CONCATENATED MODULE: ./src/events.ts\nvar events = {\n    down: 'mousedown',\n    move: 'mousemove',\n    up: 'mouseup mouseleave'\n};\nif (typeof window !== 'undefined') {\n    if (typeof window.PointerEvent === 'function') {\n        events = {\n            down: 'pointerdown',\n            move: 'pointermove',\n            up: 'pointerup pointerleave pointercancel'\n        };\n    }\n    else if (typeof window.TouchEvent === 'function') {\n        events = {\n            down: 'touchstart',\n            move: 'touchmove',\n            up: 'touchend touchcancel'\n        };\n    }\n}\n\nfunction onPointer(event, elem, handler, eventOpts) {\n    events[event].split(' ').forEach(function (name) {\n        ;\n        elem.addEventListener(name, handler, eventOpts);\n    });\n}\nfunction destroyPointer(event, elem, handler) {\n    events[event].split(' ').forEach(function (name) {\n        ;\n        elem.removeEventListener(name, handler);\n    });\n}\n\n;// CONCATENATED MODULE: ./src/css.ts\nvar isIE = typeof document !== 'undefined' && !!document.documentMode;\n/**\n * Lazy creation of a CSS style declaration\n */\nvar divStyle;\nfunction createStyle() {\n    if (divStyle) {\n        return divStyle;\n    }\n    return (divStyle = document.createElement('div').style);\n}\n/**\n * Proper prefixing for cross-browser compatibility\n */\nvar prefixes = ['webkit', 'moz', 'ms'];\nvar prefixCache = {};\nfunction getPrefixedName(name) {\n    if (prefixCache[name]) {\n        return prefixCache[name];\n    }\n    var divStyle = createStyle();\n    if (name in divStyle) {\n        return (prefixCache[name] = name);\n    }\n    var capName = name[0].toUpperCase() + name.slice(1);\n    var i = prefixes.length;\n    while (i--) {\n        var prefixedName = \"\" + prefixes[i] + capName;\n        if (prefixedName in divStyle) {\n            return (prefixCache[name] = prefixedName);\n        }\n    }\n}\n/**\n * Gets a style value expected to be a number\n */\nfunction getCSSNum(name, style) {\n    return parseFloat(style[getPrefixedName(name)]) || 0;\n}\nfunction getBoxStyle(elem, name, style) {\n    if (style === void 0) { style = window.getComputedStyle(elem); }\n    // Support: FF 68+\n    // Firefox requires specificity for border\n    var suffix = name === 'border' ? 'Width' : '';\n    return {\n        left: getCSSNum(name + \"Left\" + suffix, style),\n        right: getCSSNum(name + \"Right\" + suffix, style),\n        top: getCSSNum(name + \"Top\" + suffix, style),\n        bottom: getCSSNum(name + \"Bottom\" + suffix, style)\n    };\n}\n/**\n * Set a style using the properly prefixed name\n */\nfunction setStyle(elem, name, value) {\n    // eslint-disable-next-line @typescript-eslint/no-explicit-any\n    elem.style[getPrefixedName(name)] = value;\n}\n/**\n * Constructs the transition from panzoom options\n * and takes care of prefixing the transition and transform\n */\nfunction setTransition(elem, options) {\n    var transform = getPrefixedName('transform');\n    setStyle(elem, 'transition', transform + \" \" + options.duration + \"ms \" + options.easing);\n}\n/**\n * Set the transform using the proper prefix\n *\n * Override the transform setter.\n * This is exposed mostly so the user could\n * set other parts of a transform\n * aside from scale and translate.\n * Default is defined in src/css.ts.\n *\n * ```js\n * // This example always sets a rotation\n * // when setting the scale and translation\n * const panzoom = Panzoom(elem, {\n *   setTransform: (elem, { scale, x, y }) => {\n *     panzoom.setStyle('transform', `rotate(0.5turn) scale(${scale}) translate(${x}px, ${y}px)`)\n *   }\n * })\n * ```\n */\nfunction setTransform(elem, _a, _options) {\n    var x = _a.x, y = _a.y, scale = _a.scale, isSVG = _a.isSVG;\n    setStyle(elem, 'transform', \"scale(\" + scale + \") translate(\" + x + \"px, \" + y + \"px)\");\n    if (isSVG && isIE) {\n        var matrixValue = window.getComputedStyle(elem).getPropertyValue('transform');\n        elem.setAttribute('transform', matrixValue);\n    }\n}\n/**\n * Dimensions used in containment and focal point zooming\n */\nfunction getDimensions(elem) {\n    var parent = elem.parentNode;\n    var style = window.getComputedStyle(elem);\n    var parentStyle = window.getComputedStyle(parent);\n    var rectElem = elem.getBoundingClientRect();\n    var rectParent = parent.getBoundingClientRect();\n    return {\n        elem: {\n            style: style,\n            width: rectElem.width,\n            height: rectElem.height,\n            top: rectElem.top,\n            bottom: rectElem.bottom,\n            left: rectElem.left,\n            right: rectElem.right,\n            margin: getBoxStyle(elem, 'margin', style),\n            border: getBoxStyle(elem, 'border', style)\n        },\n        parent: {\n            style: parentStyle,\n            width: rectParent.width,\n            height: rectParent.height,\n            top: rectParent.top,\n            bottom: rectParent.bottom,\n            left: rectParent.left,\n            right: rectParent.right,\n            padding: getBoxStyle(parent, 'padding', parentStyle),\n            border: getBoxStyle(parent, 'border', parentStyle)\n        }\n    };\n}\n\n;// CONCATENATED MODULE: ./src/isAttached.ts\n/**\n * Determine if an element is attached to the DOM\n * Panzoom requires this so events work properly\n */\nfunction isAttached(elem) {\n    var doc = elem.ownerDocument;\n    var parent = elem.parentNode;\n    return (doc &&\n        parent &&\n        doc.nodeType === 9 &&\n        parent.nodeType === 1 &&\n        doc.documentElement.contains(parent));\n}\n\n;// CONCATENATED MODULE: ./src/isExcluded.ts\nfunction getClass(elem) {\n    return (elem.getAttribute('class') || '').trim();\n}\nfunction hasClass(elem, className) {\n    return elem.nodeType === 1 && (\" \" + getClass(elem) + \" \").indexOf(\" \" + className + \" \") > -1;\n}\nfunction isExcluded(elem, options) {\n    for (var cur = elem; cur != null; cur = cur.parentNode) {\n        if (hasClass(cur, options.excludeClass) || options.exclude.indexOf(cur) > -1) {\n            return true;\n        }\n    }\n    return false;\n}\n\n;// CONCATENATED MODULE: ./src/isSVGElement.ts\n/**\n * Determine if an element is SVG by checking the namespace\n * Exception: the <svg> element itself should be treated like HTML\n */\nvar rsvg = /^http:[\\w\\.\\/]+svg$/;\nfunction isSVGElement(elem) {\n    return rsvg.test(elem.namespaceURI) && elem.nodeName.toLowerCase() !== 'svg';\n}\n\n;// CONCATENATED MODULE: ./src/shallowClone.ts\nfunction shallowClone(obj) {\n    var clone = {};\n    for (var key in obj) {\n        if (obj.hasOwnProperty(key)) {\n            clone[key] = obj[key];\n        }\n    }\n    return clone;\n}\n\n;// CONCATENATED MODULE: ./src/panzoom.ts\nvar __assign = (undefined && undefined.__assign) || function () {\n    __assign = Object.assign || function(t) {\n        for (var s, i = 1, n = arguments.length; i < n; i++) {\n            s = arguments[i];\n            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))\n                t[p] = s[p];\n        }\n        return t;\n    };\n    return __assign.apply(this, arguments);\n};\n/**\n * Panzoom for panning and zooming elements using CSS transforms\n * https://github.com/timmywil/panzoom\n *\n * Copyright Timmy Willison and other contributors\n * Released under the MIT license\n * https://github.com/timmywil/panzoom/blob/main/MIT-License.txt\n *\n */\n\n\n\n\n\n\n\n\nvar defaultOptions = {\n    animate: false,\n    canvas: false,\n    cursor: 'move',\n    disablePan: false,\n    disableZoom: false,\n    disableXAxis: false,\n    disableYAxis: false,\n    duration: 200,\n    easing: 'ease-in-out',\n    exclude: [],\n    excludeClass: 'panzoom-exclude',\n    handleStartEvent: function (e) {\n        e.preventDefault();\n        e.stopPropagation();\n    },\n    maxScale: 4,\n    minScale: 0.125,\n    overflow: 'hidden',\n    panOnlyWhenZoomed: false,\n    relative: false,\n    setTransform: setTransform,\n    startX: 0,\n    startY: 0,\n    startScale: 1,\n    step: 0.3,\n    touchAction: 'none'\n};\nfunction Panzoom(elem, options) {\n    if (!elem) {\n        throw new Error('Panzoom requires an element as an argument');\n    }\n    if (elem.nodeType !== 1) {\n        throw new Error('Panzoom requires an element with a nodeType of 1');\n    }\n    if (!isAttached(elem)) {\n        throw new Error('Panzoom should be called on elements that have been attached to the DOM');\n    }\n    options = __assign(__assign({}, defaultOptions), options);\n    var isSVG = isSVGElement(elem);\n    var parent = elem.parentNode;\n    // Set parent styles\n    parent.style.overflow = options.overflow;\n    parent.style.userSelect = 'none';\n    // This is important for mobile to\n    // prevent scrolling while panning\n    parent.style.touchAction = options.touchAction;\n    (options.canvas ? parent : elem).style.cursor = options.cursor;\n    // Set element styles\n    elem.style.userSelect = 'none';\n    elem.style.touchAction = options.touchAction;\n    // The default for HTML is '50% 50%'\n    // The default for SVG is '0 0'\n    // SVG can't be changed in IE\n    setStyle(elem, 'transformOrigin', typeof options.origin === 'string' ? options.origin : isSVG ? '0 0' : '50% 50%');\n    function resetStyle() {\n        parent.style.overflow = '';\n        parent.style.userSelect = '';\n        parent.style.touchAction = '';\n        parent.style.cursor = '';\n        elem.style.cursor = '';\n        elem.style.userSelect = '';\n        elem.style.touchAction = '';\n        setStyle(elem, 'transformOrigin', '');\n    }\n    function setOptions(opts) {\n        if (opts === void 0) { opts = {}; }\n        for (var key in opts) {\n            if (opts.hasOwnProperty(key)) {\n                options[key] = opts[key];\n            }\n        }\n        // Handle option side-effects\n        if (opts.hasOwnProperty('cursor') || opts.hasOwnProperty('canvas')) {\n            parent.style.cursor = elem.style.cursor = '';\n            (options.canvas ? parent : elem).style.cursor = options.cursor;\n        }\n        if (opts.hasOwnProperty('overflow')) {\n            parent.style.overflow = opts.overflow;\n        }\n        if (opts.hasOwnProperty('touchAction')) {\n            parent.style.touchAction = opts.touchAction;\n            elem.style.touchAction = opts.touchAction;\n        }\n        if (opts.hasOwnProperty('minScale') ||\n            opts.hasOwnProperty('maxScale') ||\n            opts.hasOwnProperty('contain')) {\n            setMinMax();\n        }\n    }\n    var x = 0;\n    var y = 0;\n    var scale = 1;\n    var isPanning = false;\n    zoom(options.startScale, { animate: false, force: true });\n    // Wait for scale to update\n    // for accurate dimensions\n    // to constrain initial values\n    setTimeout(function () {\n        setMinMax();\n        pan(options.startX, options.startY, { animate: false, force: true });\n    });\n    function trigger(eventName, detail, opts) {\n        if (opts.silent) {\n            return;\n        }\n        var event = new CustomEvent(eventName, { detail: detail });\n        elem.dispatchEvent(event);\n    }\n    function setTransformWithEvent(eventName, opts, originalEvent) {\n        var value = { x: x, y: y, scale: scale, isSVG: isSVG, originalEvent: originalEvent };\n        requestAnimationFrame(function () {\n            if (typeof opts.animate === 'boolean') {\n                if (opts.animate) {\n                    setTransition(elem, opts);\n                }\n                else {\n                    setStyle(elem, 'transition', 'none');\n                }\n            }\n            opts.setTransform(elem, value, opts);\n            trigger(eventName, value, opts);\n            trigger('panzoomchange', value, opts);\n        });\n        return value;\n    }\n    function setMinMax() {\n        if (options.contain) {\n            var dims = getDimensions(elem);\n            var parentWidth = dims.parent.width - dims.parent.border.left - dims.parent.border.right;\n            var parentHeight = dims.parent.height - dims.parent.border.top - dims.parent.border.bottom;\n            var elemWidth = dims.elem.width / scale;\n            var elemHeight = dims.elem.height / scale;\n            var elemScaledWidth = parentWidth / elemWidth;\n            var elemScaledHeight = parentHeight / elemHeight;\n            if (options.contain === 'inside') {\n                options.maxScale = Math.min(elemScaledWidth, elemScaledHeight);\n            }\n            else if (options.contain === 'outside') {\n                options.minScale = Math.max(elemScaledWidth, elemScaledHeight);\n            }\n        }\n    }\n    function constrainXY(toX, toY, toScale, panOptions) {\n        var opts = __assign(__assign({}, options), panOptions);\n        var result = { x: x, y: y, opts: opts };\n        if (!opts.force && (opts.disablePan || (opts.panOnlyWhenZoomed && scale === opts.startScale))) {\n            return result;\n        }\n        toX = parseFloat(toX);\n        toY = parseFloat(toY);\n        if (!opts.disableXAxis) {\n            result.x = (opts.relative ? x : 0) + toX;\n        }\n        if (!opts.disableYAxis) {\n            result.y = (opts.relative ? y : 0) + toY;\n        }\n        if (opts.contain) {\n            var dims = getDimensions(elem);\n            var realWidth = dims.elem.width / scale;\n            var realHeight = dims.elem.height / scale;\n            var scaledWidth = realWidth * toScale;\n            var scaledHeight = realHeight * toScale;\n            var diffHorizontal = (scaledWidth - realWidth) / 2;\n            var diffVertical = (scaledHeight - realHeight) / 2;\n            if (opts.contain === 'inside') {\n                var minX = (-dims.elem.margin.left - dims.parent.padding.left + diffHorizontal) / toScale;\n                var maxX = (dims.parent.width -\n                    scaledWidth -\n                    dims.parent.padding.left -\n                    dims.elem.margin.left -\n                    dims.parent.border.left -\n                    dims.parent.border.right +\n                    diffHorizontal) /\n                    toScale;\n                result.x = Math.max(Math.min(result.x, maxX), minX);\n                var minY = (-dims.elem.margin.top - dims.parent.padding.top + diffVertical) / toScale;\n                var maxY = (dims.parent.height -\n                    scaledHeight -\n                    dims.parent.padding.top -\n                    dims.elem.margin.top -\n                    dims.parent.border.top -\n                    dims.parent.border.bottom +\n                    diffVertical) /\n                    toScale;\n                result.y = Math.max(Math.min(result.y, maxY), minY);\n            }\n            else if (opts.contain === 'outside') {\n                var minX = (-(scaledWidth - dims.parent.width) -\n                    dims.parent.padding.left -\n                    dims.parent.border.left -\n                    dims.parent.border.right +\n                    diffHorizontal) /\n                    toScale;\n                var maxX = (diffHorizontal - dims.parent.padding.left) / toScale;\n                result.x = Math.max(Math.min(result.x, maxX), minX);\n                var minY = (-(scaledHeight - dims.parent.height) -\n                    dims.parent.padding.top -\n                    dims.parent.border.top -\n                    dims.parent.border.bottom +\n                    diffVertical) /\n                    toScale;\n                var maxY = (diffVertical - dims.parent.padding.top) / toScale;\n                result.y = Math.max(Math.min(result.y, maxY), minY);\n            }\n        }\n        return result;\n    }\n    function constrainScale(toScale, zoomOptions) {\n        var opts = __assign(__assign({}, options), zoomOptions);\n        var result = { scale: scale, opts: opts };\n        if (!opts.force && opts.disableZoom) {\n            return result;\n        }\n        result.scale = Math.min(Math.max(toScale, opts.minScale), opts.maxScale);\n        return result;\n    }\n    function pan(toX, toY, panOptions, originalEvent) {\n        var result = constrainXY(toX, toY, scale, panOptions);\n        var opts = result.opts;\n        x = result.x;\n        y = result.y;\n        return setTransformWithEvent('panzoompan', opts, originalEvent);\n    }\n    function zoom(toScale, zoomOptions, originalEvent) {\n        var result = constrainScale(toScale, zoomOptions);\n        var opts = result.opts;\n        if (!opts.force && opts.disableZoom) {\n            return;\n        }\n        toScale = result.scale;\n        var toX = x;\n        var toY = y;\n        if (opts.focal) {\n            // The difference between the point after the scale and the point before the scale\n            // plus the current translation after the scale\n            // neutralized to no scale (as the transform scale will apply to the translation)\n            var focal = opts.focal;\n            toX = (focal.x / toScale - focal.x / scale + x * toScale) / toScale;\n            toY = (focal.y / toScale - focal.y / scale + y * toScale) / toScale;\n        }\n        var panResult = constrainXY(toX, toY, toScale, { relative: false, force: true });\n        x = panResult.x;\n        y = panResult.y;\n        scale = toScale;\n        return setTransformWithEvent('panzoomzoom', opts, originalEvent);\n    }\n    function zoomInOut(isIn, zoomOptions) {\n        var opts = __assign(__assign(__assign({}, options), { animate: true }), zoomOptions);\n        return zoom(scale * Math.exp((isIn ? 1 : -1) * opts.step), opts);\n    }\n    function zoomIn(zoomOptions) {\n        return zoomInOut(true, zoomOptions);\n    }\n    function zoomOut(zoomOptions) {\n        return zoomInOut(false, zoomOptions);\n    }\n    function zoomToPoint(toScale, point, zoomOptions, originalEvent) {\n        var dims = getDimensions(elem);\n        // Instead of thinking of operating on the panzoom element,\n        // think of operating on the area inside the panzoom\n        // element's parent\n        // Subtract padding and border\n        var effectiveArea = {\n            width: dims.parent.width -\n                dims.parent.padding.left -\n                dims.parent.padding.right -\n                dims.parent.border.left -\n                dims.parent.border.right,\n            height: dims.parent.height -\n                dims.parent.padding.top -\n                dims.parent.padding.bottom -\n                dims.parent.border.top -\n                dims.parent.border.bottom\n        };\n        // Adjust the clientX/clientY to ignore the area\n        // outside the effective area\n        var clientX = point.clientX -\n            dims.parent.left -\n            dims.parent.padding.left -\n            dims.parent.border.left -\n            dims.elem.margin.left;\n        var clientY = point.clientY -\n            dims.parent.top -\n            dims.parent.padding.top -\n            dims.parent.border.top -\n            dims.elem.margin.top;\n        // Adjust the clientX/clientY for HTML elements,\n        // because they have a transform-origin of 50% 50%\n        if (!isSVG) {\n            clientX -= dims.elem.width / scale / 2;\n            clientY -= dims.elem.height / scale / 2;\n        }\n        // Convert the mouse point from it's position over the\n        // effective area before the scale to the position\n        // over the effective area after the scale.\n        var focal = {\n            x: (clientX / effectiveArea.width) * (effectiveArea.width * toScale),\n            y: (clientY / effectiveArea.height) * (effectiveArea.height * toScale)\n        };\n        return zoom(toScale, __assign(__assign({ animate: false }, zoomOptions), { focal: focal }), originalEvent);\n    }\n    function zoomWithWheel(event, zoomOptions) {\n        // Need to prevent the default here\n        // or it conflicts with regular page scroll\n        event.preventDefault();\n        var opts = __assign(__assign(__assign({}, options), zoomOptions), { animate: false });\n        // Normalize to deltaX in case shift modifier is used on Mac\n        var delta = event.deltaY === 0 && event.deltaX ? event.deltaX : event.deltaY;\n        var wheel = delta < 0 ? 1 : -1;\n        var toScale = constrainScale(scale * Math.exp((wheel * opts.step) / 3), opts).scale;\n        return zoomToPoint(toScale, event, opts);\n    }\n    function reset(resetOptions) {\n        var opts = __assign(__assign(__assign({}, options), { animate: true, force: true }), resetOptions);\n        scale = constrainScale(opts.startScale, opts).scale;\n        var panResult = constrainXY(opts.startX, opts.startY, scale, opts);\n        x = panResult.x;\n        y = panResult.y;\n        return setTransformWithEvent('panzoomreset', opts);\n    }\n    var origX;\n    var origY;\n    var startClientX;\n    var startClientY;\n    var startScale;\n    var startDistance;\n    var pointers = [];\n    function handleDown(event) {\n        // Don't handle this event if the target is excluded\n        if (isExcluded(event.target, options)) {\n            return;\n        }\n        addPointer(pointers, event);\n        isPanning = true;\n        options.handleStartEvent(event);\n        origX = x;\n        origY = y;\n        trigger('panzoomstart', { x: x, y: y, scale: scale, isSVG: isSVG, originalEvent: event }, options);\n        // This works whether there are multiple\n        // pointers or not\n        var point = getMiddle(pointers);\n        startClientX = point.clientX;\n        startClientY = point.clientY;\n        startScale = scale;\n        startDistance = getDistance(pointers);\n    }\n    function move(event) {\n        if (!isPanning ||\n            origX === undefined ||\n            origY === undefined ||\n            startClientX === undefined ||\n            startClientY === undefined) {\n            return;\n        }\n        addPointer(pointers, event);\n        var current = getMiddle(pointers);\n        if (pointers.length > 1) {\n            // A startDistance of 0 means\n            // that there weren't 2 pointers\n            // handled on start\n            if (startDistance === 0) {\n                startDistance = getDistance(pointers);\n            }\n            // Use the distance between the first 2 pointers\n            // to determine the current scale\n            var diff = getDistance(pointers) - startDistance;\n            var toScale = constrainScale((diff * options.step) / 80 + startScale).scale;\n            zoomToPoint(toScale, current);\n        }\n        else {\n            // Panning during pinch zoom can cause issues\n            // because the zoom has not always rendered in time\n            // for accurate calculations\n            // See https://github.com/timmywil/panzoom/issues/512\n            pan(origX + (current.clientX - startClientX) / scale, origY + (current.clientY - startClientY) / scale, {\n                animate: false\n            }, event);\n        }\n    }\n    function handleUp(event) {\n        // Don't call panzoomend when panning with 2 touches\n        // until both touches end\n        if (pointers.length === 1) {\n            trigger('panzoomend', { x: x, y: y, scale: scale, isSVG: isSVG, originalEvent: event }, options);\n        }\n        // Note: don't remove all pointers\n        // Can restart without having to reinitiate all of them\n        // Remove the pointer regardless of the isPanning state\n        removePointer(pointers, event);\n        if (!isPanning) {\n            return;\n        }\n        isPanning = false;\n        origX = origY = startClientX = startClientY = undefined;\n    }\n    var bound = false;\n    function bind() {\n        if (bound) {\n            return;\n        }\n        bound = true;\n        onPointer('down', options.canvas ? parent : elem, handleDown);\n        onPointer('move', document, move, { passive: true });\n        onPointer('up', document, handleUp, { passive: true });\n    }\n    function destroy() {\n        bound = false;\n        destroyPointer('down', options.canvas ? parent : elem, handleDown);\n        destroyPointer('move', document, move);\n        destroyPointer('up', document, handleUp);\n    }\n    if (!options.noBind) {\n        bind();\n    }\n    return {\n        bind: bind,\n        destroy: destroy,\n        eventNames: events,\n        getPan: function () { return ({ x: x, y: y }); },\n        getScale: function () { return scale; },\n        getOptions: function () { return shallowClone(options); },\n        pan: pan,\n        reset: reset,\n        resetStyle: resetStyle,\n        setOptions: setOptions,\n        setStyle: function (name, value) { return setStyle(elem, name, value); },\n        zoom: zoom,\n        zoomIn: zoomIn,\n        zoomOut: zoomOut,\n        zoomToPoint: zoomToPoint,\n        zoomWithWheel: zoomWithWheel\n    };\n}\nPanzoom.defaultOptions = defaultOptions;\n/* harmony default export */ const panzoom = (Panzoom);\n//# sourceURL=[module]\n//# sourceMappingURL=data:application/json;charset=utf-8;base64,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QU87UUFDZCxLQUFLLEdBQUcsS0FBSztRQUNiLGNBQWMsQ0FBQyxNQUFNLEVBQUUsT0FBTyxDQUFDLE1BQU0sQ0FBQyxDQUFDLENBQUMsTUFBTSxDQUFDLENBQUMsQ0FBQyxJQUFJLEVBQUUsVUFBVSxDQUFDO1FBQ2xFLGNBQWMsQ0FBQyxNQUFNLEVBQUUsUUFBUSxFQUFFLElBQUksQ0FBQztRQUN0QyxjQUFjLENBQUMsSUFBSSxFQUFFLFFBQVEsRUFBRSxRQUFRLENBQUM7SUFDMUMsQ0FBQztJQUVELElBQUksQ0FBQyxPQUFPLENBQUMsTUFBTSxFQUFFO1FBQ25CLElBQUksRUFBRTtLQUNQO0lBRUQsT0FBTztRQUNMLElBQUk7UUFDSixPQUFPO1FBQ1AsVUFBVTtRQUNWLE1BQU0sRUFBRSxjQUFNLFFBQUMsRUFBRSxDQUFDLEtBQUUsQ0FBQyxLQUFFLENBQUMsRUFBVixDQUFVO1FBQ3hCLFFBQVEsRUFBRSxjQUFNLFlBQUssRUFBTCxDQUFLO1FBQ3JCLFVBQVUsRUFBRSxjQUFNLG1CQUFZLENBQUMsT0FBTyxDQUFDLEVBQXJCLENBQXFCO1FBQ3ZDLEdBQUc7UUFDSCxLQUFLO1FBQ0wsVUFBVTtRQUNWLFVBQVU7UUFDVixRQUFRLEVBQUUsVUFBQyxJQUFZLEVBQUUsS0FBYSxJQUFLLGVBQVEsQ0FBQyxJQUFJLEVBQUUsSUFBSSxFQUFFLEtBQUssQ0FBQyxFQUEzQixDQUEyQjtRQUN0RSxJQUFJO1FBQ0osTUFBTTtRQUNOLE9BQU87UUFDUCxXQUFXO1FBQ1gsYUFBYTtLQUNkO0FBQ0gsQ0FBQztBQUVELE9BQU8sQ0FBQyxjQUFjLEdBQUcsY0FBYztBQUd2Qyw4Q0FBZSxPQUFPIiwic291cmNlcyI6WyJ3ZWJwYWNrOi8vQHBhbnpvb20vcGFuem9vbS8uL3NyYy9wb2ludGVycy50cz9kMDU4Iiwid2VicGFjazovL0BwYW56b29tL3Bhbnpvb20vLi9zcmMvZXZlbnRzLnRzPzA1ZDEiLCJ3ZWJwYWNrOi8vQHBhbnpvb20vcGFuem9vbS8uL3NyYy9jc3MudHM/ODE0MyIsIndlYnBhY2s6Ly9AcGFuem9vbS9wYW56b29tLy4vc3JjL2lzQXR0YWNoZWQudHM/YTAzYiIsIndlYnBhY2s6Ly9AcGFuem9vbS9wYW56b29tLy4vc3JjL2lzRXhjbHVkZWQudHM/NTQzMiIsIndlYnBhY2s6Ly9AcGFuem9vbS9wYW56b29tLy4vc3JjL2lzU1ZHRWxlbWVudC50cz80NjI4Iiwid2VicGFjazovL0BwYW56b29tL3Bhbnpvb20vLi9zcmMvc2hhbGxvd0Nsb25lLnRzP2U0NzYiLCJ3ZWJwYWNrOi8vQHBhbnpvb20vcGFuem9vbS8uL3NyYy9wYW56b29tLnRzP2I4ZjQiXSwic291cmNlc0NvbnRlbnQiOlsiLyoqXG4gKiBVdGlsaXRlcyBmb3Igd29ya2luZyB3aXRoIG11bHRpcGxlIHBvaW50ZXIgZXZlbnRzXG4gKi9cblxuZnVuY3Rpb24gZmluZEV2ZW50SW5kZXgocG9pbnRlcnM6IFBvaW50ZXJFdmVudFtdLCBldmVudDogUG9pbnRlckV2ZW50KSB7XG4gIGxldCBpID0gcG9pbnRlcnMubGVuZ3RoXG4gIHdoaWxlIChpLS0pIHtcbiAgICBpZiAocG9pbnRlcnNbaV0ucG9pbnRlcklkID09PSBldmVudC5wb2ludGVySWQpIHtcbiAgICAgIHJldHVybiBpXG4gICAgfVxuICB9XG4gIHJldHVybiAtMVxufVxuXG5leHBvcnQgZnVuY3Rpb24gYWRkUG9pbnRlcihwb2ludGVyczogUG9pbnRlckV2ZW50W10sIGV2ZW50OiBQb2ludGVyRXZlbnQpIHtcbiAgbGV0IGlcbiAgLy8gQWRkIHRvdWNoZXMgaWYgYXBwbGljYWJsZVxuICBpZiAoKGV2ZW50IGFzIGFueSkudG91Y2hlcykge1xuICAgIGkgPSAwXG4gICAgZm9yIChjb25zdCB0b3VjaCBvZiAoZXZlbnQgYXMgYW55KS50b3VjaGVzKSB7XG4gICAgICB0b3VjaC5wb2ludGVySWQgPSBpKytcbiAgICAgIGFkZFBvaW50ZXIocG9pbnRlcnMsIHRvdWNoKVxuICAgIH1cbiAgICByZXR1cm5cbiAgfVxuICBpID0gZmluZEV2ZW50SW5kZXgocG9pbnRlcnMsIGV2ZW50KVxuICAvLyBVcGRhdGUgaWYgYWxyZWFkeSBwcmVzZW50XG4gIGlmIChpID4gLTEpIHtcbiAgICBwb2ludGVycy5zcGxpY2UoaSwgMSlcbiAgfVxuICBwb2ludGVycy5wdXNoKGV2ZW50KVxufVxuXG5leHBvcnQgZnVuY3Rpb24gcmVtb3ZlUG9pbnRlcihwb2ludGVyczogUG9pbnRlckV2ZW50W10sIGV2ZW50OiBQb2ludGVyRXZlbnQpIHtcbiAgLy8gQWRkIHRvdWNoZXMgaWYgYXBwbGljYWJsZVxuICBpZiAoKGV2ZW50IGFzIGFueSkudG91Y2hlcykge1xuICAgIC8vIFJlbW92ZSBhbGwgdG91Y2hlc1xuICAgIHdoaWxlIChwb2ludGVycy5sZW5ndGgpIHtcbiAgICAgIHBvaW50ZXJzLnBvcCgpXG4gICAgfVxuICAgIHJldHVyblxuICB9XG4gIGNvbnN0IGkgPSBmaW5kRXZlbnRJbmRleChwb2ludGVycywgZXZlbnQpXG4gIGlmIChpID4gLTEpIHtcbiAgICBwb2ludGVycy5zcGxpY2UoaSwgMSlcbiAgfVxufVxuXG4vKipcbiAqIENhbGN1bGF0ZXMgYSBjZW50ZXIgcG9pbnQgYmV0d2VlblxuICogdGhlIGdpdmVuIHBvaW50ZXIgZXZlbnRzLCBmb3IgcGFubmluZ1xuICogd2l0aCBtdWx0aXBsZSBwb2ludGVycy5cbiAqL1xuZXhwb3J0IGZ1bmN0aW9uIGdldE1pZGRsZShwb2ludGVyczogUG9pbnRlckV2ZW50W10pIHtcbiAgLy8gQ29weSB0byBhdm9pZCBjaGFuZ2luZyBieSByZWZlcmVuY2VcbiAgcG9pbnRlcnMgPSBwb2ludGVycy5zbGljZSgwKVxuICBsZXQgZXZlbnQxOiBQaWNrPFBvaW50ZXJFdmVudCwgJ2NsaWVudFgnIHwgJ2NsaWVudFknPiA9IHBvaW50ZXJzLnBvcCgpXG4gIGxldCBldmVudDI6IFBvaW50ZXJFdmVudFxuICB3aGlsZSAoKGV2ZW50MiA9IHBvaW50ZXJzLnBvcCgpKSkge1xuICAgIGV2ZW50MSA9IHtcbiAgICAgIGNsaWVudFg6IChldmVudDIuY2xpZW50WCAtIGV2ZW50MS5jbGllbnRYKSAvIDIgKyBldmVudDEuY2xpZW50WCxcbiAgICAgIGNsaWVudFk6IChldmVudDIuY2xpZW50WSAtIGV2ZW50MS5jbGllbnRZKSAvIDIgKyBldmVudDEuY2xpZW50WVxuICAgIH1cbiAgfVxuICByZXR1cm4gZXZlbnQxXG59XG5cbi8qKlxuICogQ2FsY3VsYXRlcyB0aGUgZGlzdGFuY2UgYmV0d2VlbiB0d28gcG9pbnRzXG4gKiBmb3IgcGluY2ggem9vbWluZy5cbiAqIExpbWl0cyB0byB0aGUgZmlyc3QgMlxuICovXG5leHBvcnQgZnVuY3Rpb24gZ2V0RGlzdGFuY2UocG9pbnRlcnM6IFBvaW50ZXJFdmVudFtdKSB7XG4gIGlmIChwb2ludGVycy5sZW5ndGggPCAyKSB7XG4gICAgcmV0dXJuIDBcbiAgfVxuICBjb25zdCBldmVudDEgPSBwb2ludGVyc1swXVxuICBjb25zdCBldmVudDIgPSBwb2ludGVyc1sxXVxuICByZXR1cm4gTWF0aC5zcXJ0KFxuICAgIE1hdGgucG93KE1hdGguYWJzKGV2ZW50Mi5jbGllbnRYIC0gZXZlbnQxLmNsaWVudFgpLCAyKSArXG4gICAgICBNYXRoLnBvdyhNYXRoLmFicyhldmVudDIuY2xpZW50WSAtIGV2ZW50MS5jbGllbnRZKSwgMilcbiAgKVxufVxuIiwibGV0IGV2ZW50cyA9IHtcbiAgZG93bjogJ21vdXNlZG93bicsXG4gIG1vdmU6ICdtb3VzZW1vdmUnLFxuICB1cDogJ21vdXNldXAgbW91c2VsZWF2ZSdcbn1cblxuaWYgKHR5cGVvZiB3aW5kb3cgIT09ICd1bmRlZmluZWQnKSB7XG4gIGlmICh0eXBlb2Ygd2luZG93LlBvaW50ZXJFdmVudCA9PT0gJ2Z1bmN0aW9uJykge1xuICAgIGV2ZW50cyA9IHtcbiAgICAgIGRvd246ICdwb2ludGVyZG93bicsXG4gICAgICBtb3ZlOiAncG9pbnRlcm1vdmUnLFxuICAgICAgdXA6ICdwb2ludGVydXAgcG9pbnRlcmxlYXZlIHBvaW50ZXJjYW5jZWwnXG4gICAgfVxuICB9IGVsc2UgaWYgKHR5cGVvZiB3aW5kb3cuVG91Y2hFdmVudCA9PT0gJ2Z1bmN0aW9uJykge1xuICAgIGV2ZW50cyA9IHtcbiAgICAgIGRvd246ICd0b3VjaHN0YXJ0JyxcbiAgICAgIG1vdmU6ICd0b3VjaG1vdmUnLFxuICAgICAgdXA6ICd0b3VjaGVuZCB0b3VjaGNhbmNlbCdcbiAgICB9XG4gIH1cbn1cblxuZXhwb3J0IHsgZXZlbnRzIGFzIGV2ZW50TmFtZXMgfVxuXG50eXBlIFBvaW50ZXJFdmVudE5hbWUgPVxuICB8ICdwb2ludGVyZG93bidcbiAgfCAncG9pbnRlcm1vdmUnXG4gIHwgJ3BvaW50ZXJ1cCdcbiAgfCAncG9pbnRlcmxlYXZlJ1xuICB8ICdwb2ludGVyY2FuY2VsJ1xuXG5leHBvcnQgZnVuY3Rpb24gb25Qb2ludGVyKFxuICBldmVudDogJ2Rvd24nIHwgJ21vdmUnIHwgJ3VwJyxcbiAgZWxlbTogSFRNTEVsZW1lbnQgfCBTVkdFbGVtZW50IHwgRG9jdW1lbnQsXG4gIGhhbmRsZXI6IChldmVudDogUG9pbnRlckV2ZW50KSA9PiB2b2lkLFxuICBldmVudE9wdHM/OiBib29sZWFuIHwgQWRkRXZlbnRMaXN0ZW5lck9wdGlvbnNcbikge1xuICBldmVudHNbZXZlbnRdLnNwbGl0KCcgJykuZm9yRWFjaCgobmFtZSkgPT4ge1xuICAgIDsoZWxlbSBhcyBIVE1MRWxlbWVudCkuYWRkRXZlbnRMaXN0ZW5lcjxQb2ludGVyRXZlbnROYW1lPihcbiAgICAgIG5hbWUgYXMgUG9pbnRlckV2ZW50TmFtZSxcbiAgICAgIGhhbmRsZXIsXG4gICAgICBldmVudE9wdHNcbiAgICApXG4gIH0pXG59XG5cbmV4cG9ydCBmdW5jdGlvbiBkZXN0cm95UG9pbnRlcihcbiAgZXZlbnQ6ICdkb3duJyB8ICdtb3ZlJyB8ICd1cCcsXG4gIGVsZW06IEhUTUxFbGVtZW50IHwgU1ZHRWxlbWVudCB8IERvY3VtZW50LFxuICBoYW5kbGVyOiAoZXZlbnQ6IFBvaW50ZXJFdmVudCkgPT4gdm9pZFxuKSB7XG4gIGV2ZW50c1tldmVudF0uc3BsaXQoJyAnKS5mb3JFYWNoKChuYW1lKSA9PiB7XG4gICAgOyhlbGVtIGFzIEhUTUxFbGVtZW50KS5yZW1vdmVFdmVudExpc3RlbmVyPFBvaW50ZXJFdmVudE5hbWU+KG5hbWUgYXMgUG9pbnRlckV2ZW50TmFtZSwgaGFuZGxlcilcbiAgfSlcbn1cbiIsImltcG9ydCB7IEN1cnJlbnRWYWx1ZXMsIFBhbnpvb21PcHRpb25zIH0gZnJvbSAnLi90eXBlcydcblxuY29uc3QgaXNJRSA9IHR5cGVvZiBkb2N1bWVudCAhPT0gJ3VuZGVmaW5lZCcgJiYgISEoZG9jdW1lbnQgYXMgYW55KS5kb2N1bWVudE1vZGVcblxuLyoqXG4gKiBMYXp5IGNyZWF0aW9uIG9mIGEgQ1NTIHN0eWxlIGRlY2xhcmF0aW9uXG4gKi9cbmxldCBkaXZTdHlsZTogQ1NTU3R5bGVEZWNsYXJhdGlvblxuZnVuY3Rpb24gY3JlYXRlU3R5bGUoKSB7XG4gIGlmIChkaXZTdHlsZSkge1xuICAgIHJldHVybiBkaXZTdHlsZVxuICB9XG4gIHJldHVybiAoZGl2U3R5bGUgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KCdkaXYnKS5zdHlsZSlcbn1cblxuLyoqXG4gKiBQcm9wZXIgcHJlZml4aW5nIGZvciBjcm9zcy1icm93c2VyIGNvbXBhdGliaWxpdHlcbiAqL1xuY29uc3QgcHJlZml4ZXMgPSBbJ3dlYmtpdCcsICdtb3onLCAnbXMnXVxuY29uc3QgcHJlZml4Q2FjaGU6IHsgW2tleTogc3RyaW5nXTogc3RyaW5nIH0gPSB7fVxuZnVuY3Rpb24gZ2V0UHJlZml4ZWROYW1lKG5hbWU6IHN0cmluZykge1xuICBpZiAocHJlZml4Q2FjaGVbbmFtZV0pIHtcbiAgICByZXR1cm4gcHJlZml4Q2FjaGVbbmFtZV1cbiAgfVxuICBjb25zdCBkaXZTdHlsZSA9IGNyZWF0ZVN0eWxlKClcbiAgaWYgKG5hbWUgaW4gZGl2U3R5bGUpIHtcbiAgICByZXR1cm4gKHByZWZpeENhY2hlW25hbWVdID0gbmFtZSlcbiAgfVxuICBjb25zdCBjYXBOYW1lID0gbmFtZVswXS50b1VwcGVyQ2FzZSgpICsgbmFtZS5zbGljZSgxKVxuICBsZXQgaSA9IHByZWZpeGVzLmxlbmd0aFxuICB3aGlsZSAoaS0tKSB7XG4gICAgY29uc3QgcHJlZml4ZWROYW1lID0gYCR7cHJlZml4ZXNbaV19JHtjYXBOYW1lfWBcbiAgICBpZiAocHJlZml4ZWROYW1lIGluIGRpdlN0eWxlKSB7XG4gICAgICByZXR1cm4gKHByZWZpeENhY2hlW25hbWVdID0gcHJlZml4ZWROYW1lKVxuICAgIH1cbiAgfVxufVxuXG4vKipcbiAqIEdldHMgYSBzdHlsZSB2YWx1ZSBleHBlY3RlZCB0byBiZSBhIG51bWJlclxuICovXG5leHBvcnQgZnVuY3Rpb24gZ2V0Q1NTTnVtKG5hbWU6IHN0cmluZywgc3R5bGU6IENTU1N0eWxlRGVjbGFyYXRpb24pIHtcbiAgcmV0dXJuIHBhcnNlRmxvYXQoc3R5bGVbZ2V0UHJlZml4ZWROYW1lKG5hbWUpIGFzIGFueV0pIHx8IDBcbn1cblxuZnVuY3Rpb24gZ2V0Qm94U3R5bGUoXG4gIGVsZW06IEhUTUxFbGVtZW50IHwgU1ZHRWxlbWVudCxcbiAgbmFtZTogc3RyaW5nLFxuICBzdHlsZTogQ1NTU3R5bGVEZWNsYXJhdGlvbiA9IHdpbmRvdy5nZXRDb21wdXRlZFN0eWxlKGVsZW0pXG4pIHtcbiAgLy8gU3VwcG9ydDogRkYgNjgrXG4gIC8vIEZpcmVmb3ggcmVxdWlyZXMgc3BlY2lmaWNpdHkgZm9yIGJvcmRlclxuICBjb25zdCBzdWZmaXggPSBuYW1lID09PSAnYm9yZGVyJyA/ICdXaWR0aCcgOiAnJ1xuICByZXR1cm4ge1xuICAgIGxlZnQ6IGdldENTU051bShgJHtuYW1lfUxlZnQke3N1ZmZpeH1gLCBzdHlsZSksXG4gICAgcmlnaHQ6IGdldENTU051bShgJHtuYW1lfVJpZ2h0JHtzdWZmaXh9YCwgc3R5bGUpLFxuICAgIHRvcDogZ2V0Q1NTTnVtKGAke25hbWV9VG9wJHtzdWZmaXh9YCwgc3R5bGUpLFxuICAgIGJvdHRvbTogZ2V0Q1NTTnVtKGAke25hbWV9Qm90dG9tJHtzdWZmaXh9YCwgc3R5bGUpXG4gIH1cbn1cblxuLyoqXG4gKiBTZXQgYSBzdHlsZSB1c2luZyB0aGUgcHJvcGVybHkgcHJlZml4ZWQgbmFtZVxuICovXG5leHBvcnQgZnVuY3Rpb24gc2V0U3R5bGUoZWxlbTogSFRNTEVsZW1lbnQgfCBTVkdFbGVtZW50LCBuYW1lOiBzdHJpbmcsIHZhbHVlOiBzdHJpbmcpIHtcbiAgLy8gZXNsaW50LWRpc2FibGUtbmV4dC1saW5lIEB0eXBlc2NyaXB0LWVzbGludC9uby1leHBsaWNpdC1hbnlcbiAgZWxlbS5zdHlsZVtnZXRQcmVmaXhlZE5hbWUobmFtZSkgYXMgYW55XSA9IHZhbHVlXG59XG5cbi8qKlxuICogQ29uc3RydWN0cyB0aGUgdHJhbnNpdGlvbiBmcm9tIHBhbnpvb20gb3B0aW9uc1xuICogYW5kIHRha2VzIGNhcmUgb2YgcHJlZml4aW5nIHRoZSB0cmFuc2l0aW9uIGFuZCB0cmFuc2Zvcm1cbiAqL1xuZXhwb3J0IGZ1bmN0aW9uIHNldFRyYW5zaXRpb24oZWxlbTogSFRNTEVsZW1lbnQgfCBTVkdFbGVtZW50LCBvcHRpb25zOiBQYW56b29tT3B0aW9ucykge1xuICBjb25zdCB0cmFuc2Zvcm0gPSBnZXRQcmVmaXhlZE5hbWUoJ3RyYW5zZm9ybScpXG4gIHNldFN0eWxlKGVsZW0sICd0cmFuc2l0aW9uJywgYCR7dHJhbnNmb3JtfSAke29wdGlvbnMuZHVyYXRpb259bXMgJHtvcHRpb25zLmVhc2luZ31gKVxufVxuXG4vKipcbiAqIFNldCB0aGUgdHJhbnNmb3JtIHVzaW5nIHRoZSBwcm9wZXIgcHJlZml4XG4gKlxuICogT3ZlcnJpZGUgdGhlIHRyYW5zZm9ybSBzZXR0ZXIuXG4gKiBUaGlzIGlzIGV4cG9zZWQgbW9zdGx5IHNvIHRoZSB1c2VyIGNvdWxkXG4gKiBzZXQgb3RoZXIgcGFydHMgb2YgYSB0cmFuc2Zvcm1cbiAqIGFzaWRlIGZyb20gc2NhbGUgYW5kIHRyYW5zbGF0ZS5cbiAqIERlZmF1bHQgaXMgZGVmaW5lZCBpbiBzcmMvY3NzLnRzLlxuICpcbiAqIGBgYGpzXG4gKiAvLyBUaGlzIGV4YW1wbGUgYWx3YXlzIHNldHMgYSByb3RhdGlvblxuICogLy8gd2hlbiBzZXR0aW5nIHRoZSBzY2FsZSBhbmQgdHJhbnNsYXRpb25cbiAqIGNvbnN0IHBhbnpvb20gPSBQYW56b29tKGVsZW0sIHtcbiAqICAgc2V0VHJhbnNmb3JtOiAoZWxlbSwgeyBzY2FsZSwgeCwgeSB9KSA9PiB7XG4gKiAgICAgcGFuem9vbS5zZXRTdHlsZSgndHJhbnNmb3JtJywgYHJvdGF0ZSgwLjV0dXJuKSBzY2FsZSgke3NjYWxlfSkgdHJhbnNsYXRlKCR7eH1weCwgJHt5fXB4KWApXG4gKiAgIH1cbiAqIH0pXG4gKiBgYGBcbiAqL1xuZXhwb3J0IGZ1bmN0aW9uIHNldFRyYW5zZm9ybShcbiAgZWxlbTogSFRNTEVsZW1lbnQgfCBTVkdFbGVtZW50LFxuICB7IHgsIHksIHNjYWxlLCBpc1NWRyB9OiBDdXJyZW50VmFsdWVzLFxuICBfb3B0aW9ucz86IFBhbnpvb21PcHRpb25zXG4pIHtcbiAgc2V0U3R5bGUoZWxlbSwgJ3RyYW5zZm9ybScsIGBzY2FsZSgke3NjYWxlfSkgdHJhbnNsYXRlKCR7eH1weCwgJHt5fXB4KWApXG4gIGlmIChpc1NWRyAmJiBpc0lFKSB7XG4gICAgY29uc3QgbWF0cml4VmFsdWUgPSB3aW5kb3cuZ2V0Q29tcHV0ZWRTdHlsZShlbGVtKS5nZXRQcm9wZXJ0eVZhbHVlKCd0cmFuc2Zvcm0nKVxuICAgIGVsZW0uc2V0QXR0cmlidXRlKCd0cmFuc2Zvcm0nLCBtYXRyaXhWYWx1ZSlcbiAgfVxufVxuXG4vKipcbiAqIERpbWVuc2lvbnMgdXNlZCBpbiBjb250YWlubWVudCBhbmQgZm9jYWwgcG9pbnQgem9vbWluZ1xuICovXG5leHBvcnQgZnVuY3Rpb24gZ2V0RGltZW5zaW9ucyhlbGVtOiBIVE1MRWxlbWVudCB8IFNWR0VsZW1lbnQpIHtcbiAgY29uc3QgcGFyZW50ID0gZWxlbS5wYXJlbnROb2RlIGFzIEhUTUxFbGVtZW50IHwgU1ZHRWxlbWVudFxuICBjb25zdCBzdHlsZSA9IHdpbmRvdy5nZXRDb21wdXRlZFN0eWxlKGVsZW0pXG4gIGNvbnN0IHBhcmVudFN0eWxlID0gd2luZG93LmdldENvbXB1dGVkU3R5bGUocGFyZW50KVxuICBjb25zdCByZWN0RWxlbSA9IGVsZW0uZ2V0Qm91bmRpbmdDbGllbnRSZWN0KClcbiAgY29uc3QgcmVjdFBhcmVudCA9IHBhcmVudC5nZXRCb3VuZGluZ0NsaWVudFJlY3QoKVxuXG4gIHJldHVybiB7XG4gICAgZWxlbToge1xuICAgICAgc3R5bGUsXG4gICAgICB3aWR0aDogcmVjdEVsZW0ud2lkdGgsXG4gICAgICBoZWlnaHQ6IHJlY3RFbGVtLmhlaWdodCxcbiAgICAgIHRvcDogcmVjdEVsZW0udG9wLFxuICAgICAgYm90dG9tOiByZWN0RWxlbS5ib3R0b20sXG4gICAgICBsZWZ0OiByZWN0RWxlbS5sZWZ0LFxuICAgICAgcmlnaHQ6IHJlY3RFbGVtLnJpZ2h0LFxuICAgICAgbWFyZ2luOiBnZXRCb3hTdHlsZShlbGVtLCAnbWFyZ2luJywgc3R5bGUpLFxuICAgICAgYm9yZGVyOiBnZXRCb3hTdHlsZShlbGVtLCAnYm9yZGVyJywgc3R5bGUpXG4gICAgfSxcbiAgICBwYXJlbnQ6IHtcbiAgICAgIHN0eWxlOiBwYXJlbnRTdHlsZSxcbiAgICAgIHdpZHRoOiByZWN0UGFyZW50LndpZHRoLFxuICAgICAgaGVpZ2h0OiByZWN0UGFyZW50LmhlaWdodCxcbiAgICAgIHRvcDogcmVjdFBhcmVudC50b3AsXG4gICAgICBib3R0b206IHJlY3RQYXJlbnQuYm90dG9tLFxuICAgICAgbGVmdDogcmVjdFBhcmVudC5sZWZ0LFxuICAgICAgcmlnaHQ6IHJlY3RQYXJlbnQucmlnaHQsXG4gICAgICBwYWRkaW5nOiBnZXRCb3hTdHlsZShwYXJlbnQsICdwYWRkaW5nJywgcGFyZW50U3R5bGUpLFxuICAgICAgYm9yZGVyOiBnZXRCb3hTdHlsZShwYXJlbnQsICdib3JkZXInLCBwYXJlbnRTdHlsZSlcbiAgICB9XG4gIH1cbn1cbiIsIi8qKlxuICogRGV0ZXJtaW5lIGlmIGFuIGVsZW1lbnQgaXMgYXR0YWNoZWQgdG8gdGhlIERPTVxuICogUGFuem9vbSByZXF1aXJlcyB0aGlzIHNvIGV2ZW50cyB3b3JrIHByb3Blcmx5XG4gKi9cbmV4cG9ydCBkZWZhdWx0IGZ1bmN0aW9uIGlzQXR0YWNoZWQoZWxlbTogSFRNTEVsZW1lbnQgfCBTVkdFbGVtZW50IHwgRG9jdW1lbnQpIHtcbiAgY29uc3QgZG9jID0gZWxlbS5vd25lckRvY3VtZW50XG4gIGNvbnN0IHBhcmVudCA9IGVsZW0ucGFyZW50Tm9kZVxuICByZXR1cm4gKFxuICAgIGRvYyAmJlxuICAgIHBhcmVudCAmJlxuICAgIGRvYy5ub2RlVHlwZSA9PT0gOSAmJlxuICAgIHBhcmVudC5ub2RlVHlwZSA9PT0gMSAmJlxuICAgIGRvYy5kb2N1bWVudEVsZW1lbnQuY29udGFpbnMocGFyZW50KVxuICApXG59XG4iLCJpbXBvcnQgeyBQYW56b29tT3B0aW9ucyB9IGZyb20gJy4vdHlwZXMnXG5cbmZ1bmN0aW9uIGdldENsYXNzKGVsZW06IEVsZW1lbnQpIHtcbiAgcmV0dXJuIChlbGVtLmdldEF0dHJpYnV0ZSgnY2xhc3MnKSB8fCAnJykudHJpbSgpXG59XG5cbmZ1bmN0aW9uIGhhc0NsYXNzKGVsZW06IEVsZW1lbnQsIGNsYXNzTmFtZTogc3RyaW5nKSB7XG4gIHJldHVybiBlbGVtLm5vZGVUeXBlID09PSAxICYmIGAgJHtnZXRDbGFzcyhlbGVtKX0gYC5pbmRleE9mKGAgJHtjbGFzc05hbWV9IGApID4gLTFcbn1cblxuZXhwb3J0IGRlZmF1bHQgZnVuY3Rpb24gaXNFeGNsdWRlZChlbGVtOiBFbGVtZW50LCBvcHRpb25zOiBQYW56b29tT3B0aW9ucykge1xuICBmb3IgKGxldCBjdXIgPSBlbGVtOyBjdXIgIT0gbnVsbDsgY3VyID0gY3VyLnBhcmVudE5vZGUgYXMgRWxlbWVudCkge1xuICAgIGlmIChoYXNDbGFzcyhjdXIsIG9wdGlvbnMuZXhjbHVkZUNsYXNzKSB8fCBvcHRpb25zLmV4Y2x1ZGUuaW5kZXhPZihjdXIpID4gLTEpIHtcbiAgICAgIHJldHVybiB0cnVlXG4gICAgfVxuICB9XG4gIHJldHVybiBmYWxzZVxufVxuIiwiLyoqXG4gKiBEZXRlcm1pbmUgaWYgYW4gZWxlbWVudCBpcyBTVkcgYnkgY2hlY2tpbmcgdGhlIG5hbWVzcGFjZVxuICogRXhjZXB0aW9uOiB0aGUgPHN2Zz4gZWxlbWVudCBpdHNlbGYgc2hvdWxkIGJlIHRyZWF0ZWQgbGlrZSBIVE1MXG4gKi9cbmNvbnN0IHJzdmcgPSAvXmh0dHA6W1xcd1xcLlxcL10rc3ZnJC9cbmV4cG9ydCBkZWZhdWx0IGZ1bmN0aW9uIGlzU1ZHRWxlbWVudChlbGVtOiBIVE1MRWxlbWVudCB8IFNWR0VsZW1lbnQpIHtcbiAgcmV0dXJuIHJzdmcudGVzdChlbGVtLm5hbWVzcGFjZVVSSSkgJiYgZWxlbS5ub2RlTmFtZS50b0xvd2VyQ2FzZSgpICE9PSAnc3ZnJ1xufVxuIiwiZXhwb3J0IGRlZmF1bHQgZnVuY3Rpb24gc2hhbGxvd0Nsb25lKG9iajogYW55KSB7XG4gIGNvbnN0IGNsb25lOiBhbnkgPSB7fVxuICBmb3IgKGNvbnN0IGtleSBpbiBvYmopIHtcbiAgICBpZiAob2JqLmhhc093blByb3BlcnR5KGtleSkpIHtcbiAgICAgIGNsb25lW2tleV0gPSBvYmpba2V5XVxuICAgIH1cbiAgfVxuICByZXR1cm4gY2xvbmVcbn1cbiIsIi8qKlxuICogUGFuem9vbSBmb3IgcGFubmluZyBhbmQgem9vbWluZyBlbGVtZW50cyB1c2luZyBDU1MgdHJhbnNmb3Jtc1xuICogaHR0cHM6Ly9naXRodWIuY29tL3RpbW15d2lsL3Bhbnpvb21cbiAqXG4gKiBDb3B5cmlnaHQgVGltbXkgV2lsbGlzb24gYW5kIG90aGVyIGNvbnRyaWJ1dG9yc1xuICogUmVsZWFzZWQgdW5kZXIgdGhlIE1JVCBsaWNlbnNlXG4gKiBodHRwczovL2dpdGh1Yi5jb20vdGltbXl3aWwvcGFuem9vbS9ibG9iL21haW4vTUlULUxpY2Vuc2UudHh0XG4gKlxuICovXG5pbXBvcnQgJy4vcG9seWZpbGxzJ1xuXG5pbXBvcnQge1xuICBQYW5PcHRpb25zLFxuICBQYW56b29tRXZlbnQsXG4gIFBhbnpvb21FdmVudERldGFpbCxcbiAgUGFuem9vbU9iamVjdCxcbiAgUGFuem9vbU9wdGlvbnMsXG4gIFpvb21PcHRpb25zXG59IGZyb20gJy4vdHlwZXMnXG5pbXBvcnQgeyBhZGRQb2ludGVyLCBnZXREaXN0YW5jZSwgZ2V0TWlkZGxlLCByZW1vdmVQb2ludGVyIH0gZnJvbSAnLi9wb2ludGVycydcbmltcG9ydCB7IGRlc3Ryb3lQb2ludGVyLCBldmVudE5hbWVzLCBvblBvaW50ZXIgfSBmcm9tICcuL2V2ZW50cydcbmltcG9ydCB7IGdldERpbWVuc2lvbnMsIHNldFN0eWxlLCBzZXRUcmFuc2Zvcm0sIHNldFRyYW5zaXRpb24gfSBmcm9tICcuL2NzcydcblxuaW1wb3J0IGlzQXR0YWNoZWQgZnJvbSAnLi9pc0F0dGFjaGVkJ1xuaW1wb3J0IGlzRXhjbHVkZWQgZnJvbSAnLi9pc0V4Y2x1ZGVkJ1xuaW1wb3J0IGlzU1ZHRWxlbWVudCBmcm9tICcuL2lzU1ZHRWxlbWVudCdcbmltcG9ydCBzaGFsbG93Q2xvbmUgZnJvbSAnLi9zaGFsbG93Q2xvbmUnXG5cbmNvbnN0IGRlZmF1bHRPcHRpb25zOiBQYW56b29tT3B0aW9ucyA9IHtcbiAgYW5pbWF0ZTogZmFsc2UsXG4gIGNhbnZhczogZmFsc2UsXG4gIGN1cnNvcjogJ21vdmUnLFxuICBkaXNhYmxlUGFuOiBmYWxzZSxcbiAgZGlzYWJsZVpvb206IGZhbHNlLFxuICBkaXNhYmxlWEF4aXM6IGZhbHNlLFxuICBkaXNhYmxlWUF4aXM6IGZhbHNlLFxuICBkdXJhdGlvbjogMjAwLFxuICBlYXNpbmc6ICdlYXNlLWluLW91dCcsXG4gIGV4Y2x1ZGU6IFtdLFxuICBleGNsdWRlQ2xhc3M6ICdwYW56b29tLWV4Y2x1ZGUnLFxuICBoYW5kbGVTdGFydEV2ZW50OiAoZTogRXZlbnQpID0+IHtcbiAgICBlLnByZXZlbnREZWZhdWx0KClcbiAgICBlLnN0b3BQcm9wYWdhdGlvbigpXG4gIH0sXG4gIG1heFNjYWxlOiA0LFxuICBtaW5TY2FsZTogMC4xMjUsXG4gIG92ZXJmbG93OiAnaGlkZGVuJyxcbiAgcGFuT25seVdoZW5ab29tZWQ6IGZhbHNlLFxuICByZWxhdGl2ZTogZmFsc2UsXG4gIHNldFRyYW5zZm9ybSxcbiAgc3RhcnRYOiAwLFxuICBzdGFydFk6IDAsXG4gIHN0YXJ0U2NhbGU6IDEsXG4gIHN0ZXA6IDAuMyxcbiAgdG91Y2hBY3Rpb246ICdub25lJ1xufVxuXG5mdW5jdGlvbiBQYW56b29tKFxuICBlbGVtOiBIVE1MRWxlbWVudCB8IFNWR0VsZW1lbnQsXG4gIG9wdGlvbnM/OiBPbWl0PFBhbnpvb21PcHRpb25zLCAnZm9yY2UnPlxuKTogUGFuem9vbU9iamVjdCB7XG4gIGlmICghZWxlbSkge1xuICAgIHRocm93IG5ldyBFcnJvcignUGFuem9vbSByZXF1aXJlcyBhbiBlbGVtZW50IGFzIGFuIGFyZ3VtZW50JylcbiAgfVxuICBpZiAoZWxlbS5ub2RlVHlwZSAhPT0gMSkge1xuICAgIHRocm93IG5ldyBFcnJvcignUGFuem9vbSByZXF1aXJlcyBhbiBlbGVtZW50IHdpdGggYSBub2RlVHlwZSBvZiAxJylcbiAgfVxuICBpZiAoIWlzQXR0YWNoZWQoZWxlbSkpIHtcbiAgICB0aHJvdyBuZXcgRXJyb3IoJ1Bhbnpvb20gc2hvdWxkIGJlIGNhbGxlZCBvbiBlbGVtZW50cyB0aGF0IGhhdmUgYmVlbiBhdHRhY2hlZCB0byB0aGUgRE9NJylcbiAgfVxuXG4gIG9wdGlvbnMgPSB7XG4gICAgLi4uZGVmYXVsdE9wdGlvbnMsXG4gICAgLi4ub3B0aW9uc1xuICB9XG5cbiAgY29uc3QgaXNTVkcgPSBpc1NWR0VsZW1lbnQoZWxlbSlcblxuICBjb25zdCBwYXJlbnQgPSBlbGVtLnBhcmVudE5vZGUgYXMgSFRNTEVsZW1lbnQgfCBTVkdFbGVtZW50XG5cbiAgLy8gU2V0IHBhcmVudCBzdHlsZXNcbiAgcGFyZW50LnN0eWxlLm92ZXJmbG93ID0gb3B0aW9ucy5vdmVyZmxvd1xuICBwYXJlbnQuc3R5bGUudXNlclNlbGVjdCA9ICdub25lJ1xuICAvLyBUaGlzIGlzIGltcG9ydGFudCBmb3IgbW9iaWxlIHRvXG4gIC8vIHByZXZlbnQgc2Nyb2xsaW5nIHdoaWxlIHBhbm5pbmdcbiAgcGFyZW50LnN0eWxlLnRvdWNoQWN0aW9uID0gb3B0aW9ucy50b3VjaEFjdGlvblxuICAvLyBTZXQgdGhlIGN1cnNvciBzdHlsZSBvbiB0aGUgcGFyZW50IGlmIHdlJ3JlIGluIGNhbnZhcyBtb2RlXG4gIDsob3B0aW9ucy5jYW52YXMgPyBwYXJlbnQgOiBlbGVtKS5zdHlsZS5jdXJzb3IgPSBvcHRpb25zLmN1cnNvclxuXG4gIC8vIFNldCBlbGVtZW50IHN0eWxlc1xuICBlbGVtLnN0eWxlLnVzZXJTZWxlY3QgPSAnbm9uZSdcbiAgZWxlbS5zdHlsZS50b3VjaEFjdGlvbiA9IG9wdGlvbnMudG91Y2hBY3Rpb25cbiAgLy8gVGhlIGRlZmF1bHQgZm9yIEhUTUwgaXMgJzUwJSA1MCUnXG4gIC8vIFRoZSBkZWZhdWx0IGZvciBTVkcgaXMgJzAgMCdcbiAgLy8gU1ZHIGNhbid0IGJlIGNoYW5nZWQgaW4gSUVcbiAgc2V0U3R5bGUoXG4gICAgZWxlbSxcbiAgICAndHJhbnNmb3JtT3JpZ2luJyxcbiAgICB0eXBlb2Ygb3B0aW9ucy5vcmlnaW4gPT09ICdzdHJpbmcnID8gb3B0aW9ucy5vcmlnaW4gOiBpc1NWRyA/ICcwIDAnIDogJzUwJSA1MCUnXG4gIClcblxuICBmdW5jdGlvbiByZXNldFN0eWxlKCkge1xuICAgIHBhcmVudC5zdHlsZS5vdmVyZmxvdyA9ICcnXG4gICAgcGFyZW50LnN0eWxlLnVzZXJTZWxlY3QgPSAnJ1xuICAgIHBhcmVudC5zdHlsZS50b3VjaEFjdGlvbiA9ICcnXG4gICAgcGFyZW50LnN0eWxlLmN1cnNvciA9ICcnXG4gICAgZWxlbS5zdHlsZS5jdXJzb3IgPSAnJ1xuICAgIGVsZW0uc3R5bGUudXNlclNlbGVjdCA9ICcnXG4gICAgZWxlbS5zdHlsZS50b3VjaEFjdGlvbiA9ICcnXG4gICAgc2V0U3R5bGUoZWxlbSwgJ3RyYW5zZm9ybU9yaWdpbicsICcnKVxuICB9XG5cbiAgZnVuY3Rpb24gc2V0T3B0aW9ucyhvcHRzOiBPbWl0PFBhbnpvb21PcHRpb25zLCAnZm9yY2UnPiA9IHt9KSB7XG4gICAgZm9yIChjb25zdCBrZXkgaW4gb3B0cykge1xuICAgICAgaWYgKG9wdHMuaGFzT3duUHJvcGVydHkoa2V5KSkge1xuICAgICAgICBvcHRpb25zW2tleV0gPSBvcHRzW2tleV1cbiAgICAgIH1cbiAgICB9XG4gICAgLy8gSGFuZGxlIG9wdGlvbiBzaWRlLWVmZmVjdHNcbiAgICBpZiAob3B0cy5oYXNPd25Qcm9wZXJ0eSgnY3Vyc29yJykgfHwgb3B0cy5oYXNPd25Qcm9wZXJ0eSgnY2FudmFzJykpIHtcbiAgICAgIHBhcmVudC5zdHlsZS5jdXJzb3IgPSBlbGVtLnN0eWxlLmN1cnNvciA9ICcnXG4gICAgICA7KG9wdGlvbnMuY2FudmFzID8gcGFyZW50IDogZWxlbSkuc3R5bGUuY3Vyc29yID0gb3B0aW9ucy5jdXJzb3JcbiAgICB9XG4gICAgaWYgKG9wdHMuaGFzT3duUHJvcGVydHkoJ292ZXJmbG93JykpIHtcbiAgICAgIHBhcmVudC5zdHlsZS5vdmVyZmxvdyA9IG9wdHMub3ZlcmZsb3dcbiAgICB9XG4gICAgaWYgKG9wdHMuaGFzT3duUHJvcGVydHkoJ3RvdWNoQWN0aW9uJykpIHtcbiAgICAgIHBhcmVudC5zdHlsZS50b3VjaEFjdGlvbiA9IG9wdHMudG91Y2hBY3Rpb25cbiAgICAgIGVsZW0uc3R5bGUudG91Y2hBY3Rpb24gPSBvcHRzLnRvdWNoQWN0aW9uXG4gICAgfVxuICAgIGlmIChcbiAgICAgIG9wdHMuaGFzT3duUHJvcGVydHkoJ21pblNjYWxlJykgfHxcbiAgICAgIG9wdHMuaGFzT3duUHJvcGVydHkoJ21heFNjYWxlJykgfHxcbiAgICAgIG9wdHMuaGFzT3duUHJvcGVydHkoJ2NvbnRhaW4nKVxuICAgICkge1xuICAgICAgc2V0TWluTWF4KClcbiAgICB9XG4gIH1cblxuICBsZXQgeCA9IDBcbiAgbGV0IHkgPSAwXG4gIGxldCBzY2FsZSA9IDFcbiAgbGV0IGlzUGFubmluZyA9IGZhbHNlXG4gIHpvb20ob3B0aW9ucy5zdGFydFNjYWxlLCB7IGFuaW1hdGU6IGZhbHNlLCBmb3JjZTogdHJ1ZSB9KVxuICAvLyBXYWl0IGZvciBzY2FsZSB0byB1cGRhdGVcbiAgLy8gZm9yIGFjY3VyYXRlIGRpbWVuc2lvbnNcbiAgLy8gdG8gY29uc3RyYWluIGluaXRpYWwgdmFsdWVzXG4gIHNldFRpbWVvdXQoKCkgPT4ge1xuICAgIHNldE1pbk1heCgpXG4gICAgcGFuKG9wdGlvbnMuc3RhcnRYLCBvcHRpb25zLnN0YXJ0WSwgeyBhbmltYXRlOiBmYWxzZSwgZm9yY2U6IHRydWUgfSlcbiAgfSlcblxuICBmdW5jdGlvbiB0cmlnZ2VyKGV2ZW50TmFtZTogUGFuem9vbUV2ZW50LCBkZXRhaWw6IFBhbnpvb21FdmVudERldGFpbCwgb3B0czogUGFuem9vbU9wdGlvbnMpIHtcbiAgICBpZiAob3B0cy5zaWxlbnQpIHtcbiAgICAgIHJldHVyblxuICAgIH1cbiAgICBjb25zdCBldmVudCA9IG5ldyBDdXN0b21FdmVudChldmVudE5hbWUsIHsgZGV0YWlsIH0pXG4gICAgZWxlbS5kaXNwYXRjaEV2ZW50KGV2ZW50KVxuICB9XG5cbiAgZnVuY3Rpb24gc2V0VHJhbnNmb3JtV2l0aEV2ZW50KFxuICAgIGV2ZW50TmFtZTogUGFuem9vbUV2ZW50LFxuICAgIG9wdHM6IFBhbnpvb21PcHRpb25zLFxuICAgIG9yaWdpbmFsRXZlbnQ/OiBQYW56b29tRXZlbnREZXRhaWxbJ29yaWdpbmFsRXZlbnQnXVxuICApIHtcbiAgICBjb25zdCB2YWx1ZSA9IHsgeCwgeSwgc2NhbGUsIGlzU1ZHLCBvcmlnaW5hbEV2ZW50IH1cbiAgICByZXF1ZXN0QW5pbWF0aW9uRnJhbWUoKCkgPT4ge1xuICAgICAgaWYgKHR5cGVvZiBvcHRzLmFuaW1hdGUgPT09ICdib29sZWFuJykge1xuICAgICAgICBpZiAob3B0cy5hbmltYXRlKSB7XG4gICAgICAgICAgc2V0VHJhbnNpdGlvbihlbGVtLCBvcHRzKVxuICAgICAgICB9IGVsc2Uge1xuICAgICAgICAgIHNldFN0eWxlKGVsZW0sICd0cmFuc2l0aW9uJywgJ25vbmUnKVxuICAgICAgICB9XG4gICAgICB9XG4gICAgICBvcHRzLnNldFRyYW5zZm9ybShlbGVtLCB2YWx1ZSwgb3B0cylcbiAgICAgIHRyaWdnZXIoZXZlbnROYW1lLCB2YWx1ZSwgb3B0cylcbiAgICAgIHRyaWdnZXIoJ3Bhbnpvb21jaGFuZ2UnLCB2YWx1ZSwgb3B0cylcbiAgICB9KVxuICAgIHJldHVybiB2YWx1ZVxuICB9XG5cbiAgZnVuY3Rpb24gc2V0TWluTWF4KCkge1xuICAgIGlmIChvcHRpb25zLmNvbnRhaW4pIHtcbiAgICAgIGNvbnN0IGRpbXMgPSBnZXREaW1lbnNpb25zKGVsZW0pXG4gICAgICBjb25zdCBwYXJlbnRXaWR0aCA9IGRpbXMucGFyZW50LndpZHRoIC0gZGltcy5wYXJlbnQuYm9yZGVyLmxlZnQgLSBkaW1zLnBhcmVudC5ib3JkZXIucmlnaHRcbiAgICAgIGNvbnN0IHBhcmVudEhlaWdodCA9IGRpbXMucGFyZW50LmhlaWdodCAtIGRpbXMucGFyZW50LmJvcmRlci50b3AgLSBkaW1zLnBhcmVudC5ib3JkZXIuYm90dG9tXG4gICAgICBjb25zdCBlbGVtV2lkdGggPSBkaW1zLmVsZW0ud2lkdGggLyBzY2FsZVxuICAgICAgY29uc3QgZWxlbUhlaWdodCA9IGRpbXMuZWxlbS5oZWlnaHQgLyBzY2FsZVxuICAgICAgY29uc3QgZWxlbVNjYWxlZFdpZHRoID0gcGFyZW50V2lkdGggLyBlbGVtV2lkdGhcbiAgICAgIGNvbnN0IGVsZW1TY2FsZWRIZWlnaHQgPSBwYXJlbnRIZWlnaHQgLyBlbGVtSGVpZ2h0XG4gICAgICBpZiAob3B0aW9ucy5jb250YWluID09PSAnaW5zaWRlJykge1xuICAgICAgICBvcHRpb25zLm1heFNjYWxlID0gTWF0aC5taW4oZWxlbVNjYWxlZFdpZHRoLCBlbGVtU2NhbGVkSGVpZ2h0KVxuICAgICAgfSBlbHNlIGlmIChvcHRpb25zLmNvbnRhaW4gPT09ICdvdXRzaWRlJykge1xuICAgICAgICBvcHRpb25zLm1pblNjYWxlID0gTWF0aC5tYXgoZWxlbVNjYWxlZFdpZHRoLCBlbGVtU2NhbGVkSGVpZ2h0KVxuICAgICAgfVxuICAgIH1cbiAgfVxuXG4gIGZ1bmN0aW9uIGNvbnN0cmFpblhZKFxuICAgIHRvWDogbnVtYmVyIHwgc3RyaW5nLFxuICAgIHRvWTogbnVtYmVyIHwgc3RyaW5nLFxuICAgIHRvU2NhbGU6IG51bWJlcixcbiAgICBwYW5PcHRpb25zPzogUGFuT3B0aW9uc1xuICApIHtcbiAgICBjb25zdCBvcHRzID0geyAuLi5vcHRpb25zLCAuLi5wYW5PcHRpb25zIH1cbiAgICBjb25zdCByZXN1bHQgPSB7IHgsIHksIG9wdHMgfVxuICAgIGlmICghb3B0cy5mb3JjZSAmJiAob3B0cy5kaXNhYmxlUGFuIHx8IChvcHRzLnBhbk9ubHlXaGVuWm9vbWVkICYmIHNjYWxlID09PSBvcHRzLnN0YXJ0U2NhbGUpKSkge1xuICAgICAgcmV0dXJuIHJlc3VsdFxuICAgIH1cbiAgICB0b1ggPSBwYXJzZUZsb2F0KHRvWCBhcyBzdHJpbmcpXG4gICAgdG9ZID0gcGFyc2VGbG9hdCh0b1kgYXMgc3RyaW5nKVxuXG4gICAgaWYgKCFvcHRzLmRpc2FibGVYQXhpcykge1xuICAgICAgcmVzdWx0LnggPSAob3B0cy5yZWxhdGl2ZSA/IHggOiAwKSArIHRvWFxuICAgIH1cblxuICAgIGlmICghb3B0cy5kaXNhYmxlWUF4aXMpIHtcbiAgICAgIHJlc3VsdC55ID0gKG9wdHMucmVsYXRpdmUgPyB5IDogMCkgKyB0b1lcbiAgICB9XG5cbiAgICBpZiAob3B0cy5jb250YWluKSB7XG4gICAgICBjb25zdCBkaW1zID0gZ2V0RGltZW5zaW9ucyhlbGVtKVxuICAgICAgY29uc3QgcmVhbFdpZHRoID0gZGltcy5lbGVtLndpZHRoIC8gc2NhbGVcbiAgICAgIGNvbnN0IHJlYWxIZWlnaHQgPSBkaW1zLmVsZW0uaGVpZ2h0IC8gc2NhbGVcbiAgICAgIGNvbnN0IHNjYWxlZFdpZHRoID0gcmVhbFdpZHRoICogdG9TY2FsZVxuICAgICAgY29uc3Qgc2NhbGVkSGVpZ2h0ID0gcmVhbEhlaWdodCAqIHRvU2NhbGVcbiAgICAgIGNvbnN0IGRpZmZIb3Jpem9udGFsID0gKHNjYWxlZFdpZHRoIC0gcmVhbFdpZHRoKSAvIDJcbiAgICAgIGNvbnN0IGRpZmZWZXJ0aWNhbCA9IChzY2FsZWRIZWlnaHQgLSByZWFsSGVpZ2h0KSAvIDJcblxuICAgICAgaWYgKG9wdHMuY29udGFpbiA9PT0gJ2luc2lkZScpIHtcbiAgICAgICAgY29uc3QgbWluWCA9ICgtZGltcy5lbGVtLm1hcmdpbi5sZWZ0IC0gZGltcy5wYXJlbnQucGFkZGluZy5sZWZ0ICsgZGlmZkhvcml6b250YWwpIC8gdG9TY2FsZVxuICAgICAgICBjb25zdCBtYXhYID1cbiAgICAgICAgICAoZGltcy5wYXJlbnQud2lkdGggLVxuICAgICAgICAgICAgc2NhbGVkV2lkdGggLVxuICAgICAgICAgICAgZGltcy5wYXJlbnQucGFkZGluZy5sZWZ0IC1cbiAgICAgICAgICAgIGRpbXMuZWxlbS5tYXJnaW4ubGVmdCAtXG4gICAgICAgICAgICBkaW1zLnBhcmVudC5ib3JkZXIubGVmdCAtXG4gICAgICAgICAgICBkaW1zLnBhcmVudC5ib3JkZXIucmlnaHQgK1xuICAgICAgICAgICAgZGlmZkhvcml6b250YWwpIC9cbiAgICAgICAgICB0b1NjYWxlXG4gICAgICAgIHJlc3VsdC54ID0gTWF0aC5tYXgoTWF0aC5taW4ocmVzdWx0LngsIG1heFgpLCBtaW5YKVxuICAgICAgICBjb25zdCBtaW5ZID0gKC1kaW1zLmVsZW0ubWFyZ2luLnRvcCAtIGRpbXMucGFyZW50LnBhZGRpbmcudG9wICsgZGlmZlZlcnRpY2FsKSAvIHRvU2NhbGVcbiAgICAgICAgY29uc3QgbWF4WSA9XG4gICAgICAgICAgKGRpbXMucGFyZW50LmhlaWdodCAtXG4gICAgICAgICAgICBzY2FsZWRIZWlnaHQgLVxuICAgICAgICAgICAgZGltcy5wYXJlbnQucGFkZGluZy50b3AgLVxuICAgICAgICAgICAgZGltcy5lbGVtLm1hcmdpbi50b3AgLVxuICAgICAgICAgICAgZGltcy5wYXJlbnQuYm9yZGVyLnRvcCAtXG4gICAgICAgICAgICBkaW1zLnBhcmVudC5ib3JkZXIuYm90dG9tICtcbiAgICAgICAgICAgIGRpZmZWZXJ0aWNhbCkgL1xuICAgICAgICAgIHRvU2NhbGVcbiAgICAgICAgcmVzdWx0LnkgPSBNYXRoLm1heChNYXRoLm1pbihyZXN1bHQueSwgbWF4WSksIG1pblkpXG4gICAgICB9IGVsc2UgaWYgKG9wdHMuY29udGFpbiA9PT0gJ291dHNpZGUnKSB7XG4gICAgICAgIGNvbnN0IG1pblggPVxuICAgICAgICAgICgtKHNjYWxlZFdpZHRoIC0gZGltcy5wYXJlbnQud2lkdGgpIC1cbiAgICAgICAgICAgIGRpbXMucGFyZW50LnBhZGRpbmcubGVmdCAtXG4gICAgICAgICAgICBkaW1zLnBhcmVudC5ib3JkZXIubGVmdCAtXG4gICAgICAgICAgICBkaW1zLnBhcmVudC5ib3JkZXIucmlnaHQgK1xuICAgICAgICAgICAgZGlmZkhvcml6b250YWwpIC9cbiAgICAgICAgICB0b1NjYWxlXG4gICAgICAgIGNvbnN0IG1heFggPSAoZGlmZkhvcml6b250YWwgLSBkaW1zLnBhcmVudC5wYWRkaW5nLmxlZnQpIC8gdG9TY2FsZVxuICAgICAgICByZXN1bHQueCA9IE1hdGgubWF4KE1hdGgubWluKHJlc3VsdC54LCBtYXhYKSwgbWluWClcbiAgICAgICAgY29uc3QgbWluWSA9XG4gICAgICAgICAgKC0oc2NhbGVkSGVpZ2h0IC0gZGltcy5wYXJlbnQuaGVpZ2h0KSAtXG4gICAgICAgICAgICBkaW1zLnBhcmVudC5wYWRkaW5nLnRvcCAtXG4gICAgICAgICAgICBkaW1zLnBhcmVudC5ib3JkZXIudG9wIC1cbiAgICAgICAgICAgIGRpbXMucGFyZW50LmJvcmRlci5ib3R0b20gK1xuICAgICAgICAgICAgZGlmZlZlcnRpY2FsKSAvXG4gICAgICAgICAgdG9TY2FsZVxuICAgICAgICBjb25zdCBtYXhZID0gKGRpZmZWZXJ0aWNhbCAtIGRpbXMucGFyZW50LnBhZGRpbmcudG9wKSAvIHRvU2NhbGVcbiAgICAgICAgcmVzdWx0LnkgPSBNYXRoLm1heChNYXRoLm1pbihyZXN1bHQueSwgbWF4WSksIG1pblkpXG4gICAgICB9XG4gICAgfVxuICAgIHJldHVybiByZXN1bHRcbiAgfVxuXG4gIGZ1bmN0aW9uIGNvbnN0cmFpblNjYWxlKHRvU2NhbGU6IG51bWJlciwgem9vbU9wdGlvbnM/OiBab29tT3B0aW9ucykge1xuICAgIGNvbnN0IG9wdHMgPSB7IC4uLm9wdGlvbnMsIC4uLnpvb21PcHRpb25zIH1cbiAgICBjb25zdCByZXN1bHQgPSB7IHNjYWxlLCBvcHRzIH1cbiAgICBpZiAoIW9wdHMuZm9yY2UgJiYgb3B0cy5kaXNhYmxlWm9vbSkge1xuICAgICAgcmV0dXJuIHJlc3VsdFxuICAgIH1cbiAgICByZXN1bHQuc2NhbGUgPSBNYXRoLm1pbihNYXRoLm1heCh0b1NjYWxlLCBvcHRzLm1pblNjYWxlKSwgb3B0cy5tYXhTY2FsZSlcbiAgICByZXR1cm4gcmVzdWx0XG4gIH1cblxuICBmdW5jdGlvbiBwYW4oXG4gICAgdG9YOiBudW1iZXIgfCBzdHJpbmcsXG4gICAgdG9ZOiBudW1iZXIgfCBzdHJpbmcsXG4gICAgcGFuT3B0aW9ucz86IFBhbk9wdGlvbnMsXG4gICAgb3JpZ2luYWxFdmVudD86IFBhbnpvb21FdmVudERldGFpbFsnb3JpZ2luYWxFdmVudCddXG4gICkge1xuICAgIGNvbnN0IHJlc3VsdCA9IGNvbnN0cmFpblhZKHRvWCwgdG9ZLCBzY2FsZSwgcGFuT3B0aW9ucylcbiAgICBjb25zdCBvcHRzID0gcmVzdWx0Lm9wdHNcblxuICAgIHggPSByZXN1bHQueFxuICAgIHkgPSByZXN1bHQueVxuXG4gICAgcmV0dXJuIHNldFRyYW5zZm9ybVdpdGhFdmVudCgncGFuem9vbXBhbicsIG9wdHMsIG9yaWdpbmFsRXZlbnQpXG4gIH1cblxuICBmdW5jdGlvbiB6b29tKFxuICAgIHRvU2NhbGU6IG51bWJlcixcbiAgICB6b29tT3B0aW9ucz86IFpvb21PcHRpb25zLFxuICAgIG9yaWdpbmFsRXZlbnQ/OiBQYW56b29tRXZlbnREZXRhaWxbJ29yaWdpbmFsRXZlbnQnXVxuICApIHtcbiAgICBjb25zdCByZXN1bHQgPSBjb25zdHJhaW5TY2FsZSh0b1NjYWxlLCB6b29tT3B0aW9ucylcbiAgICBjb25zdCBvcHRzID0gcmVzdWx0Lm9wdHNcbiAgICBpZiAoIW9wdHMuZm9yY2UgJiYgb3B0cy5kaXNhYmxlWm9vbSkge1xuICAgICAgcmV0dXJuXG4gICAgfVxuICAgIHRvU2NhbGUgPSByZXN1bHQuc2NhbGVcbiAgICBsZXQgdG9YID0geFxuICAgIGxldCB0b1kgPSB5XG5cbiAgICBpZiAob3B0cy5mb2NhbCkge1xuICAgICAgLy8gVGhlIGRpZmZlcmVuY2UgYmV0d2VlbiB0aGUgcG9pbnQgYWZ0ZXIgdGhlIHNjYWxlIGFuZCB0aGUgcG9pbnQgYmVmb3JlIHRoZSBzY2FsZVxuICAgICAgLy8gcGx1cyB0aGUgY3VycmVudCB0cmFuc2xhdGlvbiBhZnRlciB0aGUgc2NhbGVcbiAgICAgIC8vIG5ldXRyYWxpemVkIHRvIG5vIHNjYWxlIChhcyB0aGUgdHJhbnNmb3JtIHNjYWxlIHdpbGwgYXBwbHkgdG8gdGhlIHRyYW5zbGF0aW9uKVxuICAgICAgY29uc3QgZm9jYWwgPSBvcHRzLmZvY2FsXG4gICAgICB0b1ggPSAoZm9jYWwueCAvIHRvU2NhbGUgLSBmb2NhbC54IC8gc2NhbGUgKyB4ICogdG9TY2FsZSkgLyB0b1NjYWxlXG4gICAgICB0b1kgPSAoZm9jYWwueSAvIHRvU2NhbGUgLSBmb2NhbC55IC8gc2NhbGUgKyB5ICogdG9TY2FsZSkgLyB0b1NjYWxlXG4gICAgfVxuICAgIGNvbnN0IHBhblJlc3VsdCA9IGNvbnN0cmFpblhZKHRvWCwgdG9ZLCB0b1NjYWxlLCB7IHJlbGF0aXZlOiBmYWxzZSwgZm9yY2U6IHRydWUgfSlcbiAgICB4ID0gcGFuUmVzdWx0LnhcbiAgICB5ID0gcGFuUmVzdWx0LnlcbiAgICBzY2FsZSA9IHRvU2NhbGVcbiAgICByZXR1cm4gc2V0VHJhbnNmb3JtV2l0aEV2ZW50KCdwYW56b29tem9vbScsIG9wdHMsIG9yaWdpbmFsRXZlbnQpXG4gIH1cblxuICBmdW5jdGlvbiB6b29tSW5PdXQoaXNJbjogYm9vbGVhbiwgem9vbU9wdGlvbnM/OiBab29tT3B0aW9ucykge1xuICAgIGNvbnN0IG9wdHMgPSB7IC4uLm9wdGlvbnMsIGFuaW1hdGU6IHRydWUsIC4uLnpvb21PcHRpb25zIH1cbiAgICByZXR1cm4gem9vbShzY2FsZSAqIE1hdGguZXhwKChpc0luID8gMSA6IC0xKSAqIG9wdHMuc3RlcCksIG9wdHMpXG4gIH1cblxuICBmdW5jdGlvbiB6b29tSW4oem9vbU9wdGlvbnM/OiBab29tT3B0aW9ucykge1xuICAgIHJldHVybiB6b29tSW5PdXQodHJ1ZSwgem9vbU9wdGlvbnMpXG4gIH1cblxuICBmdW5jdGlvbiB6b29tT3V0KHpvb21PcHRpb25zPzogWm9vbU9wdGlvbnMpIHtcbiAgICByZXR1cm4gem9vbUluT3V0KGZhbHNlLCB6b29tT3B0aW9ucylcbiAgfVxuXG4gIGZ1bmN0aW9uIHpvb21Ub1BvaW50KFxuICAgIHRvU2NhbGU6IG51bWJlcixcbiAgICBwb2ludDogeyBjbGllbnRYOiBudW1iZXI7IGNsaWVudFk6IG51bWJlciB9LFxuICAgIHpvb21PcHRpb25zPzogWm9vbU9wdGlvbnMsXG4gICAgb3JpZ2luYWxFdmVudD86IFBhbnpvb21FdmVudERldGFpbFsnb3JpZ2luYWxFdmVudCddXG4gICkge1xuICAgIGNvbnN0IGRpbXMgPSBnZXREaW1lbnNpb25zKGVsZW0pXG5cbiAgICAvLyBJbnN0ZWFkIG9mIHRoaW5raW5nIG9mIG9wZXJhdGluZyBvbiB0aGUgcGFuem9vbSBlbGVtZW50LFxuICAgIC8vIHRoaW5rIG9mIG9wZXJhdGluZyBvbiB0aGUgYXJlYSBpbnNpZGUgdGhlIHBhbnpvb21cbiAgICAvLyBlbGVtZW50J3MgcGFyZW50XG4gICAgLy8gU3VidHJhY3QgcGFkZGluZyBhbmQgYm9yZGVyXG4gICAgY29uc3QgZWZmZWN0aXZlQXJlYSA9IHtcbiAgICAgIHdpZHRoOlxuICAgICAgICBkaW1zLnBhcmVudC53aWR0aCAtXG4gICAgICAgIGRpbXMucGFyZW50LnBhZGRpbmcubGVmdCAtXG4gICAgICAgIGRpbXMucGFyZW50LnBhZGRpbmcucmlnaHQgLVxuICAgICAgICBkaW1zLnBhcmVudC5ib3JkZXIubGVmdCAtXG4gICAgICAgIGRpbXMucGFyZW50LmJvcmRlci5yaWdodCxcbiAgICAgIGhlaWdodDpcbiAgICAgICAgZGltcy5wYXJlbnQuaGVpZ2h0IC1cbiAgICAgICAgZGltcy5wYXJlbnQucGFkZGluZy50b3AgLVxuICAgICAgICBkaW1zLnBhcmVudC5wYWRkaW5nLmJvdHRvbSAtXG4gICAgICAgIGRpbXMucGFyZW50LmJvcmRlci50b3AgLVxuICAgICAgICBkaW1zLnBhcmVudC5ib3JkZXIuYm90dG9tXG4gICAgfVxuXG4gICAgLy8gQWRqdXN0IHRoZSBjbGllbnRYL2NsaWVudFkgdG8gaWdub3JlIHRoZSBhcmVhXG4gICAgLy8gb3V0c2lkZSB0aGUgZWZmZWN0aXZlIGFyZWFcbiAgICBsZXQgY2xpZW50WCA9XG4gICAgICBwb2ludC5jbGllbnRYIC1cbiAgICAgIGRpbXMucGFyZW50LmxlZnQgLVxuICAgICAgZGltcy5wYXJlbnQucGFkZGluZy5sZWZ0IC1cbiAgICAgIGRpbXMucGFyZW50LmJvcmRlci5sZWZ0IC1cbiAgICAgIGRpbXMuZWxlbS5tYXJnaW4ubGVmdFxuICAgIGxldCBjbGllbnRZID1cbiAgICAgIHBvaW50LmNsaWVudFkgLVxuICAgICAgZGltcy5wYXJlbnQudG9wIC1cbiAgICAgIGRpbXMucGFyZW50LnBhZGRpbmcudG9wIC1cbiAgICAgIGRpbXMucGFyZW50LmJvcmRlci50b3AgLVxuICAgICAgZGltcy5lbGVtLm1hcmdpbi50b3BcblxuICAgIC8vIEFkanVzdCB0aGUgY2xpZW50WC9jbGllbnRZIGZvciBIVE1MIGVsZW1lbnRzLFxuICAgIC8vIGJlY2F1c2UgdGhleSBoYXZlIGEgdHJhbnNmb3JtLW9yaWdpbiBvZiA1MCUgNTAlXG4gICAgaWYgKCFpc1NWRykge1xuICAgICAgY2xpZW50WCAtPSBkaW1zLmVsZW0ud2lkdGggLyBzY2FsZSAvIDJcbiAgICAgIGNsaWVudFkgLT0gZGltcy5lbGVtLmhlaWdodCAvIHNjYWxlIC8gMlxuICAgIH1cblxuICAgIC8vIENvbnZlcnQgdGhlIG1vdXNlIHBvaW50IGZyb20gaXQncyBwb3NpdGlvbiBvdmVyIHRoZVxuICAgIC8vIGVmZmVjdGl2ZSBhcmVhIGJlZm9yZSB0aGUgc2NhbGUgdG8gdGhlIHBvc2l0aW9uXG4gICAgLy8gb3ZlciB0aGUgZWZmZWN0aXZlIGFyZWEgYWZ0ZXIgdGhlIHNjYWxlLlxuICAgIGNvbnN0IGZvY2FsID0ge1xuICAgICAgeDogKGNsaWVudFggLyBlZmZlY3RpdmVBcmVhLndpZHRoKSAqIChlZmZlY3RpdmVBcmVhLndpZHRoICogdG9TY2FsZSksXG4gICAgICB5OiAoY2xpZW50WSAvIGVmZmVjdGl2ZUFyZWEuaGVpZ2h0KSAqIChlZmZlY3RpdmVBcmVhLmhlaWdodCAqIHRvU2NhbGUpXG4gICAgfVxuXG4gICAgcmV0dXJuIHpvb20odG9TY2FsZSwgeyBhbmltYXRlOiBmYWxzZSwgLi4uem9vbU9wdGlvbnMsIGZvY2FsIH0sIG9yaWdpbmFsRXZlbnQpXG4gIH1cblxuICBmdW5jdGlvbiB6b29tV2l0aFdoZWVsKGV2ZW50OiBXaGVlbEV2ZW50LCB6b29tT3B0aW9ucz86IFpvb21PcHRpb25zKSB7XG4gICAgLy8gTmVlZCB0byBwcmV2ZW50IHRoZSBkZWZhdWx0IGhlcmVcbiAgICAvLyBvciBpdCBjb25mbGljdHMgd2l0aCByZWd1bGFyIHBhZ2Ugc2Nyb2xsXG4gICAgZXZlbnQucHJldmVudERlZmF1bHQoKVxuXG4gICAgY29uc3Qgb3B0cyA9IHsgLi4ub3B0aW9ucywgLi4uem9vbU9wdGlvbnMsIGFuaW1hdGU6IGZhbHNlIH1cblxuICAgIC8vIE5vcm1hbGl6ZSB0byBkZWx0YVggaW4gY2FzZSBzaGlmdCBtb2RpZmllciBpcyB1c2VkIG9uIE1hY1xuICAgIGNvbnN0IGRlbHRhID0gZXZlbnQuZGVsdGFZID09PSAwICYmIGV2ZW50LmRlbHRhWCA/IGV2ZW50LmRlbHRhWCA6IGV2ZW50LmRlbHRhWVxuICAgIGNvbnN0IHdoZWVsID0gZGVsdGEgPCAwID8gMSA6IC0xXG4gICAgY29uc3QgdG9TY2FsZSA9IGNvbnN0cmFpblNjYWxlKHNjYWxlICogTWF0aC5leHAoKHdoZWVsICogb3B0cy5zdGVwKSAvIDMpLCBvcHRzKS5zY2FsZVxuXG4gICAgcmV0dXJuIHpvb21Ub1BvaW50KHRvU2NhbGUsIGV2ZW50LCBvcHRzKVxuICB9XG5cbiAgZnVuY3Rpb24gcmVzZXQocmVzZXRPcHRpb25zPzogUGFuem9vbU9wdGlvbnMpIHtcbiAgICBjb25zdCBvcHRzID0geyAuLi5vcHRpb25zLCBhbmltYXRlOiB0cnVlLCBmb3JjZTogdHJ1ZSwgLi4ucmVzZXRPcHRpb25zIH1cbiAgICBzY2FsZSA9IGNvbnN0cmFpblNjYWxlKG9wdHMuc3RhcnRTY2FsZSwgb3B0cykuc2NhbGVcbiAgICBjb25zdCBwYW5SZXN1bHQgPSBjb25zdHJhaW5YWShvcHRzLnN0YXJ0WCwgb3B0cy5zdGFydFksIHNjYWxlLCBvcHRzKVxuICAgIHggPSBwYW5SZXN1bHQueFxuICAgIHkgPSBwYW5SZXN1bHQueVxuICAgIHJldHVybiBzZXRUcmFuc2Zvcm1XaXRoRXZlbnQoJ3Bhbnpvb21yZXNldCcsIG9wdHMpXG4gIH1cblxuICBsZXQgb3JpZ1g6IG51bWJlclxuICBsZXQgb3JpZ1k6IG51bWJlclxuICBsZXQgc3RhcnRDbGllbnRYOiBudW1iZXJcbiAgbGV0IHN0YXJ0Q2xpZW50WTogbnVtYmVyXG4gIGxldCBzdGFydFNjYWxlOiBudW1iZXJcbiAgbGV0IHN0YXJ0RGlzdGFuY2U6IG51bWJlclxuICBjb25zdCBwb2ludGVyczogUG9pbnRlckV2ZW50W10gPSBbXVxuXG4gIGZ1bmN0aW9uIGhhbmRsZURvd24oZXZlbnQ6IFBvaW50ZXJFdmVudCkge1xuICAgIC8vIERvbid0IGhhbmRsZSB0aGlzIGV2ZW50IGlmIHRoZSB0YXJnZXQgaXMgZXhjbHVkZWRcbiAgICBpZiAoaXNFeGNsdWRlZChldmVudC50YXJnZXQgYXMgRWxlbWVudCwgb3B0aW9ucykpIHtcbiAgICAgIHJldHVyblxuICAgIH1cbiAgICBhZGRQb2ludGVyKHBvaW50ZXJzLCBldmVudClcbiAgICBpc1Bhbm5pbmcgPSB0cnVlXG4gICAgb3B0aW9ucy5oYW5kbGVTdGFydEV2ZW50KGV2ZW50KVxuICAgIG9yaWdYID0geFxuICAgIG9yaWdZID0geVxuXG4gICAgdHJpZ2dlcigncGFuem9vbXN0YXJ0JywgeyB4LCB5LCBzY2FsZSwgaXNTVkcsIG9yaWdpbmFsRXZlbnQ6IGV2ZW50IH0sIG9wdGlvbnMpXG5cbiAgICAvLyBUaGlzIHdvcmtzIHdoZXRoZXIgdGhlcmUgYXJlIG11bHRpcGxlXG4gICAgLy8gcG9pbnRlcnMgb3Igbm90XG4gICAgY29uc3QgcG9pbnQgPSBnZXRNaWRkbGUocG9pbnRlcnMpXG4gICAgc3RhcnRDbGllbnRYID0gcG9pbnQuY2xpZW50WFxuICAgIHN0YXJ0Q2xpZW50WSA9IHBvaW50LmNsaWVudFlcbiAgICBzdGFydFNjYWxlID0gc2NhbGVcbiAgICBzdGFydERpc3RhbmNlID0gZ2V0RGlzdGFuY2UocG9pbnRlcnMpXG4gIH1cblxuICBmdW5jdGlvbiBtb3ZlKGV2ZW50OiBQb2ludGVyRXZlbnQpIHtcbiAgICBpZiAoXG4gICAgICAhaXNQYW5uaW5nIHx8XG4gICAgICBvcmlnWCA9PT0gdW5kZWZpbmVkIHx8XG4gICAgICBvcmlnWSA9PT0gdW5kZWZpbmVkIHx8XG4gICAgICBzdGFydENsaWVudFggPT09IHVuZGVmaW5lZCB8fFxuICAgICAgc3RhcnRDbGllbnRZID09PSB1bmRlZmluZWRcbiAgICApIHtcbiAgICAgIHJldHVyblxuICAgIH1cbiAgICBhZGRQb2ludGVyKHBvaW50ZXJzLCBldmVudClcbiAgICBjb25zdCBjdXJyZW50ID0gZ2V0TWlkZGxlKHBvaW50ZXJzKVxuICAgIGlmIChwb2ludGVycy5sZW5ndGggPiAxKSB7XG4gICAgICAvLyBBIHN0YXJ0RGlzdGFuY2Ugb2YgMCBtZWFuc1xuICAgICAgLy8gdGhhdCB0aGVyZSB3ZXJlbid0IDIgcG9pbnRlcnNcbiAgICAgIC8vIGhhbmRsZWQgb24gc3RhcnRcbiAgICAgIGlmIChzdGFydERpc3RhbmNlID09PSAwKSB7XG4gICAgICAgIHN0YXJ0RGlzdGFuY2UgPSBnZXREaXN0YW5jZShwb2ludGVycylcbiAgICAgIH1cbiAgICAgIC8vIFVzZSB0aGUgZGlzdGFuY2UgYmV0d2VlbiB0aGUgZmlyc3QgMiBwb2ludGVyc1xuICAgICAgLy8gdG8gZGV0ZXJtaW5lIHRoZSBjdXJyZW50IHNjYWxlXG4gICAgICBjb25zdCBkaWZmID0gZ2V0RGlzdGFuY2UocG9pbnRlcnMpIC0gc3RhcnREaXN0YW5jZVxuICAgICAgY29uc3QgdG9TY2FsZSA9IGNvbnN0cmFpblNjYWxlKChkaWZmICogb3B0aW9ucy5zdGVwKSAvIDgwICsgc3RhcnRTY2FsZSkuc2NhbGVcbiAgICAgIHpvb21Ub1BvaW50KHRvU2NhbGUsIGN1cnJlbnQpXG4gICAgfSBlbHNlIHtcbiAgICAgIC8vIFBhbm5pbmcgZHVyaW5nIHBpbmNoIHpvb20gY2FuIGNhdXNlIGlzc3Vlc1xuICAgICAgLy8gYmVjYXVzZSB0aGUgem9vbSBoYXMgbm90IGFsd2F5cyByZW5kZXJlZCBpbiB0aW1lXG4gICAgICAvLyBmb3IgYWNjdXJhdGUgY2FsY3VsYXRpb25zXG4gICAgICAvLyBTZWUgaHR0cHM6Ly9naXRodWIuY29tL3RpbW15d2lsL3Bhbnpvb20vaXNzdWVzLzUxMlxuICAgICAgcGFuKFxuICAgICAgICBvcmlnWCArIChjdXJyZW50LmNsaWVudFggLSBzdGFydENsaWVudFgpIC8gc2NhbGUsXG4gICAgICAgIG9yaWdZICsgKGN1cnJlbnQuY2xpZW50WSAtIHN0YXJ0Q2xpZW50WSkgLyBzY2FsZSxcbiAgICAgICAge1xuICAgICAgICAgIGFuaW1hdGU6IGZhbHNlXG4gICAgICAgIH0sXG4gICAgICAgIGV2ZW50XG4gICAgICApXG4gICAgfVxuICB9XG5cbiAgZnVuY3Rpb24gaGFuZGxlVXAoZXZlbnQ6IFBvaW50ZXJFdmVudCkge1xuICAgIC8vIERvbid0IGNhbGwgcGFuem9vbWVuZCB3aGVuIHBhbm5pbmcgd2l0aCAyIHRvdWNoZXNcbiAgICAvLyB1bnRpbCBib3RoIHRvdWNoZXMgZW5kXG4gICAgaWYgKHBvaW50ZXJzLmxlbmd0aCA9PT0gMSkge1xuICAgICAgdHJpZ2dlcigncGFuem9vbWVuZCcsIHsgeCwgeSwgc2NhbGUsIGlzU1ZHLCBvcmlnaW5hbEV2ZW50OiBldmVudCB9LCBvcHRpb25zKVxuICAgIH1cbiAgICAvLyBOb3RlOiBkb24ndCByZW1vdmUgYWxsIHBvaW50ZXJzXG4gICAgLy8gQ2FuIHJlc3RhcnQgd2l0aG91dCBoYXZpbmcgdG8gcmVpbml0aWF0ZSBhbGwgb2YgdGhlbVxuICAgIC8vIFJlbW92ZSB0aGUgcG9pbnRlciByZWdhcmRsZXNzIG9mIHRoZSBpc1Bhbm5pbmcgc3RhdGVcbiAgICByZW1vdmVQb2ludGVyKHBvaW50ZXJzLCBldmVudClcbiAgICBpZiAoIWlzUGFubmluZykge1xuICAgICAgcmV0dXJuXG4gICAgfVxuICAgIGlzUGFubmluZyA9IGZhbHNlXG4gICAgb3JpZ1ggPSBvcmlnWSA9IHN0YXJ0Q2xpZW50WCA9IHN0YXJ0Q2xpZW50WSA9IHVuZGVmaW5lZFxuICB9XG5cbiAgbGV0IGJvdW5kID0gZmFsc2VcbiAgZnVuY3Rpb24gYmluZCgpIHtcbiAgICBpZiAoYm91bmQpIHtcbiAgICAgIHJldHVyblxuICAgIH1cbiAgICBib3VuZCA9IHRydWVcbiAgICBvblBvaW50ZXIoJ2Rvd24nLCBvcHRpb25zLmNhbnZhcyA/IHBhcmVudCA6IGVsZW0sIGhhbmRsZURvd24pXG4gICAgb25Qb2ludGVyKCdtb3ZlJywgZG9jdW1lbnQsIG1vdmUsIHsgcGFzc2l2ZTogdHJ1ZSB9KVxuICAgIG9uUG9pbnRlcigndXAnLCBkb2N1bWVudCwgaGFuZGxlVXAsIHsgcGFzc2l2ZTogdHJ1ZSB9KVxuICB9XG5cbiAgZnVuY3Rpb24gZGVzdHJveSgpIHtcbiAgICBib3VuZCA9IGZhbHNlXG4gICAgZGVzdHJveVBvaW50ZXIoJ2Rvd24nLCBvcHRpb25zLmNhbnZhcyA/IHBhcmVudCA6IGVsZW0sIGhhbmRsZURvd24pXG4gICAgZGVzdHJveVBvaW50ZXIoJ21vdmUnLCBkb2N1bWVudCwgbW92ZSlcbiAgICBkZXN0cm95UG9pbnRlcigndXAnLCBkb2N1bWVudCwgaGFuZGxlVXApXG4gIH1cblxuICBpZiAoIW9wdGlvbnMubm9CaW5kKSB7XG4gICAgYmluZCgpXG4gIH1cblxuICByZXR1cm4ge1xuICAgIGJpbmQsXG4gICAgZGVzdHJveSxcbiAgICBldmVudE5hbWVzLFxuICAgIGdldFBhbjogKCkgPT4gKHsgeCwgeSB9KSxcbiAgICBnZXRTY2FsZTogKCkgPT4gc2NhbGUsXG4gICAgZ2V0T3B0aW9uczogKCkgPT4gc2hhbGxvd0Nsb25lKG9wdGlvbnMpLFxuICAgIHBhbixcbiAgICByZXNldCxcbiAgICByZXNldFN0eWxlLFxuICAgIHNldE9wdGlvbnMsXG4gICAgc2V0U3R5bGU6IChuYW1lOiBzdHJpbmcsIHZhbHVlOiBzdHJpbmcpID0+IHNldFN0eWxlKGVsZW0sIG5hbWUsIHZhbHVlKSxcbiAgICB6b29tLFxuICAgIHpvb21JbixcbiAgICB6b29tT3V0LFxuICAgIHpvb21Ub1BvaW50LFxuICAgIHpvb21XaXRoV2hlZWxcbiAgfVxufVxuXG5QYW56b29tLmRlZmF1bHRPcHRpb25zID0gZGVmYXVsdE9wdGlvbnNcblxuZXhwb3J0IHsgUGFuem9vbU9iamVjdCwgUGFuem9vbU9wdGlvbnMgfVxuZXhwb3J0IGRlZmF1bHQgUGFuem9vbVxuIl0sIm5hbWVzIjpbXSwic291cmNlUm9vdCI6IiJ9\n//# sourceURL=webpack-internal:///36\n")
        }
        ,
        252: () => {
            eval("/* eslint-disable no-var */\nif (typeof window !== 'undefined') {\n  // Support: IE11 only\n  if (window.NodeList && !NodeList.prototype.forEach) {\n    NodeList.prototype.forEach = Array.prototype.forEach\n  }\n  // Support: IE11 only\n  // CustomEvent is an object instead of a constructor\n  if (typeof window.CustomEvent !== 'function') {\n    window.CustomEvent = function CustomEvent(event, params) {\n      params = params || { bubbles: false, cancelable: false, detail: null }\n      var evt = document.createEvent('CustomEvent')\n      evt.initCustomEvent(event, params.bubbles, params.cancelable, params.detail)\n      return evt\n    }\n  }\n}\n//# sourceURL=[module]\n//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiMjUyLmpzIiwibWFwcGluZ3MiOiJBQUFBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsMkJBQTJCO0FBQzNCO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSIsInNvdXJjZXMiOlsid2VicGFjazovL0BwYW56b29tL3Bhbnpvb20vLi9zcmMvcG9seWZpbGxzLmpzPzA2NzQiXSwic291cmNlc0NvbnRlbnQiOlsiLyogZXNsaW50LWRpc2FibGUgbm8tdmFyICovXG5pZiAodHlwZW9mIHdpbmRvdyAhPT0gJ3VuZGVmaW5lZCcpIHtcbiAgLy8gU3VwcG9ydDogSUUxMSBvbmx5XG4gIGlmICh3aW5kb3cuTm9kZUxpc3QgJiYgIU5vZGVMaXN0LnByb3RvdHlwZS5mb3JFYWNoKSB7XG4gICAgTm9kZUxpc3QucHJvdG90eXBlLmZvckVhY2ggPSBBcnJheS5wcm90b3R5cGUuZm9yRWFjaFxuICB9XG4gIC8vIFN1cHBvcnQ6IElFMTEgb25seVxuICAvLyBDdXN0b21FdmVudCBpcyBhbiBvYmplY3QgaW5zdGVhZCBvZiBhIGNvbnN0cnVjdG9yXG4gIGlmICh0eXBlb2Ygd2luZG93LkN1c3RvbUV2ZW50ICE9PSAnZnVuY3Rpb24nKSB7XG4gICAgd2luZG93LkN1c3RvbUV2ZW50ID0gZnVuY3Rpb24gQ3VzdG9tRXZlbnQoZXZlbnQsIHBhcmFtcykge1xuICAgICAgcGFyYW1zID0gcGFyYW1zIHx8IHsgYnViYmxlczogZmFsc2UsIGNhbmNlbGFibGU6IGZhbHNlLCBkZXRhaWw6IG51bGwgfVxuICAgICAgdmFyIGV2dCA9IGRvY3VtZW50LmNyZWF0ZUV2ZW50KCdDdXN0b21FdmVudCcpXG4gICAgICBldnQuaW5pdEN1c3RvbUV2ZW50KGV2ZW50LCBwYXJhbXMuYnViYmxlcywgcGFyYW1zLmNhbmNlbGFibGUsIHBhcmFtcy5kZXRhaWwpXG4gICAgICByZXR1cm4gZXZ0XG4gICAgfVxuICB9XG59XG4iXSwibmFtZXMiOltdLCJzb3VyY2VSb290IjoiIn0=\n//# sourceURL=webpack-internal:///252\n")
        }
    }, __webpack_module_cache__ = {}, inProgress, dataWebpackPrefix;
    function __webpack_require__(Q) {
        var U = __webpack_module_cache__[Q];
        if (void 0 !== U) {
            if (void 0 !== U.error)
                throw U.error;
            return U.exports
        }
        var F = __webpack_module_cache__[Q] = {
            exports: {}
        };
        try {
            var B = {
                id: Q,
                module: F,
                factory: __webpack_modules__[Q],
                require: __webpack_require__
            };
            __webpack_require__.i.forEach((function (Q) {
                Q(B)
            }
            )),
                F = B.module,
                B.factory.call(F.exports, F, F.exports, B.require)
        } catch (Q) {
            throw F.error = Q,
            Q
        }
        return F.exports
    }
    __webpack_require__.m = __webpack_modules__,
        __webpack_require__.c = __webpack_module_cache__,
        __webpack_require__.i = [],
        __webpack_require__.d = (Q, U) => {
            for (var F in U)
                __webpack_require__.o(U, F) && !__webpack_require__.o(Q, F) && Object.defineProperty(Q, F, {
                    enumerable: !0,
                    get: U[F]
                })
        }
        ,
        __webpack_require__.hu = Q => Q + "." + __webpack_require__.h() + ".hot-update.js",
        __webpack_require__.hmrF = () => "panzoom." + __webpack_require__.h() + ".hot-update.json",
        __webpack_require__.h = () => "c924bc5f344e046074f0",
        __webpack_require__.g = function () {
            if ("object" == typeof globalThis)
                return globalThis;
            try {
                return this || new Function("return this")()
            } catch (Q) {
                if ("object" == typeof window)
                    return window
            }
        }(),
        __webpack_require__.o = (Q, U) => Object.prototype.hasOwnProperty.call(Q, U),
        inProgress = {},
        dataWebpackPrefix = "@panzoom/panzoom:",
        __webpack_require__.l = (Q, U, F, B) => {
            if (inProgress[Q])
                inProgress[Q].push(U);
            else {
                var n, e;
                if (void 0 !== F)
                    for (var s = document.getElementsByTagName("script"), t = 0; t < s.length; t++) {
                        var l = s[t];
                        if (l.getAttribute("src") == Q || l.getAttribute("data-webpack") == dataWebpackPrefix + F) {
                            n = l;
                            break
                        }
                    }
                n || (e = !0,
                    (n = document.createElement("script")).charset = "utf-8",
                    n.timeout = 120,
                    __webpack_require__.nc && n.setAttribute("nonce", __webpack_require__.nc),
                    n.setAttribute("data-webpack", dataWebpackPrefix + F),
                    n.src = Q),
                    inProgress[Q] = [U];
                var c = (U, F) => {
                    n.onerror = n.onload = null,
                        clearTimeout(i);
                    var B = inProgress[Q];
                    if (delete inProgress[Q],
                        n.parentNode && n.parentNode.removeChild(n),
                        B && B.forEach((Q => Q(F))),
                        U)
                        return U(F)
                }
                    , i = setTimeout(c.bind(null, void 0, {
                        type: "timeout",
                        target: n
                    }), 12e4);
                n.onerror = c.bind(null, n.onerror),
                    n.onload = c.bind(null, n.onload),
                    e && document.head.appendChild(n)
            }
        }
        ,
        (() => {
            var Q, U, F, B, n = {}, e = __webpack_require__.c, s = [], t = [], l = "idle";
            function c(Q) {
                l = Q;
                for (var U = [], F = 0; F < t.length; F++)
                    U[F] = t[F].call(null, Q);
                return Promise.all(U)
            }
            function i(Q) {
                if (0 === U.length)
                    return Q();
                var F = U;
                return U = [],
                    Promise.all(F).then((function () {
                        return i(Q)
                    }
                    ))
            }
            function g(Q) {
                if ("idle" !== l)
                    throw new Error("check() is only allowed in idle status");
                return c("check").then(__webpack_require__.hmrM).then((function (B) {
                    return B ? c("prepare").then((function () {
                        var n = [];
                        return U = [],
                            F = [],
                            Promise.all(Object.keys(__webpack_require__.hmrC).reduce((function (Q, U) {
                                return __webpack_require__.hmrC[U](B.c, B.r, B.m, Q, F, n),
                                    Q
                            }
                            ), [])).then((function () {
                                return i((function () {
                                    return Q ? d(Q) : c("ready").then((function () {
                                        return n
                                    }
                                    ))
                                }
                                ))
                            }
                            ))
                    }
                    )) : c(I() ? "ready" : "idle").then((function () {
                        return null
                    }
                    ))
                }
                ))
            }
            function b(Q) {
                return "ready" !== l ? Promise.resolve().then((function () {
                    throw new Error("apply() is only allowed in ready status")
                }
                )) : d(Q)
            }
            function d(Q) {
                Q = Q || {},
                    I();
                var U = F.map((function (U) {
                    return U(Q)
                }
                ));
                F = void 0;
                var n = U.map((function (Q) {
                    return Q.error
                }
                )).filter(Boolean);
                if (n.length > 0)
                    return c("abort").then((function () {
                        throw n[0]
                    }
                    ));
                var e = c("dispose");
                U.forEach((function (Q) {
                    Q.dispose && Q.dispose()
                }
                ));
                var s, t = c("apply"), l = function (Q) {
                    s || (s = Q)
                }, i = [];
                return U.forEach((function (Q) {
                    if (Q.apply) {
                        var U = Q.apply(l);
                        if (U)
                            for (var F = 0; F < U.length; F++)
                                i.push(U[F])
                    }
                }
                )),
                    Promise.all([e, t]).then((function () {
                        return s ? c("fail").then((function () {
                            throw s
                        }
                        )) : B ? d(Q).then((function (Q) {
                            return i.forEach((function (U) {
                                Q.indexOf(U) < 0 && Q.push(U)
                            }
                            )),
                                Q
                        }
                        )) : c("idle").then((function () {
                            return i
                        }
                        ))
                    }
                    ))
            }
            function I() {
                if (B)
                    return F || (F = []),
                        Object.keys(__webpack_require__.hmrI).forEach((function (Q) {
                            B.forEach((function (U) {
                                __webpack_require__.hmrI[Q](U, F)
                            }
                            ))
                        }
                        )),
                        B = void 0,
                        !0
            }
            __webpack_require__.hmrD = n,
                __webpack_require__.i.push((function (d) {
                    var I, x, a, o, L = d.module, r = function (F, B) {
                        var n = e[B];
                        if (!n)
                            return F;
                        var t = function (U) {
                            if (n.hot.active) {
                                if (e[U]) {
                                    var t = e[U].parents;
                                    -1 === t.indexOf(B) && t.push(B)
                                } else
                                    s = [B],
                                        Q = U;
                                -1 === n.children.indexOf(U) && n.children.push(U)
                            } else
                                console.warn("[HMR] unexpected require(" + U + ") from disposed module " + B),
                                    s = [];
                            return F(U)
                        }
                            , g = function (Q) {
                                return {
                                    configurable: !0,
                                    enumerable: !0,
                                    get: function () {
                                        return F[Q]
                                    },
                                    set: function (U) {
                                        F[Q] = U
                                    }
                                }
                            };
                        for (var b in F)
                            Object.prototype.hasOwnProperty.call(F, b) && "e" !== b && Object.defineProperty(t, b, g(b));
                        return t.e = function (Q) {
                            return function (Q) {
                                switch (l) {
                                    case "ready":
                                        return c("prepare"),
                                            U.push(Q),
                                            i((function () {
                                                return c("ready")
                                            }
                                            )),
                                            Q;
                                    case "prepare":
                                        return U.push(Q),
                                            Q;
                                    default:
                                        return Q
                                }
                            }(F.e(Q))
                        }
                            ,
                            t
                    }(d.require, d.id);
                    L.hot = (I = d.id,
                        x = L,
                        o = {
                            _acceptedDependencies: {},
                            _acceptedErrorHandlers: {},
                            _declinedDependencies: {},
                            _selfAccepted: !1,
                            _selfDeclined: !1,
                            _selfInvalidated: !1,
                            _disposeHandlers: [],
                            _main: a = Q !== I,
                            _requireSelf: function () {
                                s = x.parents.slice(),
                                    Q = a ? void 0 : I,
                                    __webpack_require__(I)
                            },
                            active: !0,
                            accept: function (Q, U, F) {
                                if (void 0 === Q)
                                    o._selfAccepted = !0;
                                else if ("function" == typeof Q)
                                    o._selfAccepted = Q;
                                else if ("object" == typeof Q && null !== Q)
                                    for (var B = 0; B < Q.length; B++)
                                        o._acceptedDependencies[Q[B]] = U || function () { }
                                            ,
                                            o._acceptedErrorHandlers[Q[B]] = F;
                                else
                                    o._acceptedDependencies[Q] = U || function () { }
                                        ,
                                        o._acceptedErrorHandlers[Q] = F
                            },
                            decline: function (Q) {
                                if (void 0 === Q)
                                    o._selfDeclined = !0;
                                else if ("object" == typeof Q && null !== Q)
                                    for (var U = 0; U < Q.length; U++)
                                        o._declinedDependencies[Q[U]] = !0;
                                else
                                    o._declinedDependencies[Q] = !0
                            },
                            dispose: function (Q) {
                                o._disposeHandlers.push(Q)
                            },
                            addDisposeHandler: function (Q) {
                                o._disposeHandlers.push(Q)
                            },
                            removeDisposeHandler: function (Q) {
                                var U = o._disposeHandlers.indexOf(Q);
                                U >= 0 && o._disposeHandlers.splice(U, 1)
                            },
                            invalidate: function () {
                                switch (this._selfInvalidated = !0,
                                l) {
                                    case "idle":
                                        F = [],
                                            Object.keys(__webpack_require__.hmrI).forEach((function (Q) {
                                                __webpack_require__.hmrI[Q](I, F)
                                            }
                                            )),
                                            c("ready");
                                        break;
                                    case "ready":
                                        Object.keys(__webpack_require__.hmrI).forEach((function (Q) {
                                            __webpack_require__.hmrI[Q](I, F)
                                        }
                                        ));
                                        break;
                                    case "prepare":
                                    case "check":
                                    case "dispose":
                                    case "apply":
                                        (B = B || []).push(I)
                                }
                            },
                            check: g,
                            apply: b,
                            status: function (Q) {
                                if (!Q)
                                    return l;
                                t.push(Q)
                            },
                            addStatusHandler: function (Q) {
                                t.push(Q)
                            },
                            removeStatusHandler: function (Q) {
                                var U = t.indexOf(Q);
                                U >= 0 && t.splice(U, 1)
                            },
                            data: n[I]
                        },
                        Q = void 0,
                        o),
                        L.parents = s,
                        L.children = [],
                        s = [],
                        d.require = r
                }
                )),
                __webpack_require__.hmrC = {},
                __webpack_require__.hmrI = {}
        }
        )(),
        (() => {
            var Q;
            __webpack_require__.g.importScripts && (Q = __webpack_require__.g.location + "");
            var U = __webpack_require__.g.document;
            if (!Q && U && (U.currentScript && (Q = U.currentScript.src),
                !Q)) {
                var F = U.getElementsByTagName("script");
                F.length && (Q = F[F.length - 1].src)
            }
            if (!Q)
                throw new Error("Automatic publicPath is not supported in this browser");
            Q = Q.replace(/#.*$/, "").replace(/\?.*$/, "").replace(/\/[^\/]+$/, "/"),
                __webpack_require__.p = Q
        }
        )(),
        (() => {
            var Q, U, F, B, n = {
                478: 0
            }, e = {};
            function s(Q) {
                return new Promise(((U, F) => {
                    e[Q] = U;
                    var B = __webpack_require__.p + __webpack_require__.hu(Q)
                        , n = new Error;
                    __webpack_require__.l(B, (U => {
                        if (e[Q]) {
                            e[Q] = void 0;
                            var B = U && ("load" === U.type ? "missing" : U.type)
                                , s = U && U.target && U.target.src;
                            n.message = "Loading hot update chunk " + Q + " failed.\n(" + B + ": " + s + ")",
                                n.name = "ChunkLoadError",
                                n.type = B,
                                n.request = s,
                                F(n)
                        }
                    }
                    ))
                }
                ))
            }
            function t(e) {
                function s(Q) {
                    for (var U = [Q], F = {}, B = U.map((function (Q) {
                        return {
                            chain: [Q],
                            id: Q
                        }
                    }
                    )); B.length > 0;) {
                        var n = B.pop()
                            , e = n.id
                            , s = n.chain
                            , l = __webpack_require__.c[e];
                        if (l && (!l.hot._selfAccepted || l.hot._selfInvalidated)) {
                            if (l.hot._selfDeclined)
                                return {
                                    type: "self-declined",
                                    chain: s,
                                    moduleId: e
                                };
                            if (l.hot._main)
                                return {
                                    type: "unaccepted",
                                    chain: s,
                                    moduleId: e
                                };
                            for (var c = 0; c < l.parents.length; c++) {
                                var i = l.parents[c]
                                    , g = __webpack_require__.c[i];
                                if (g) {
                                    if (g.hot._declinedDependencies[e])
                                        return {
                                            type: "declined",
                                            chain: s.concat([i]),
                                            moduleId: e,
                                            parentId: i
                                        };
                                    -1 === U.indexOf(i) && (g.hot._acceptedDependencies[e] ? (F[i] || (F[i] = []),
                                        t(F[i], [e])) : (delete F[i],
                                            U.push(i),
                                            B.push({
                                                chain: s.concat([i]),
                                                id: i
                                            })))
                                }
                            }
                        }
                    }
                    return {
                        type: "accepted",
                        moduleId: Q,
                        outdatedModules: U,
                        outdatedDependencies: F
                    }
                }
                function t(Q, U) {
                    for (var F = 0; F < U.length; F++) {
                        var B = U[F];
                        -1 === Q.indexOf(B) && Q.push(B)
                    }
                }
                __webpack_require__.f && delete __webpack_require__.f.jsonpHmr,
                    Q = void 0;
                var l = {}
                    , c = []
                    , i = {}
                    , g = function (Q) {
                        console.warn("[HMR] unexpected require(" + Q.id + ") to disposed module")
                    };
                for (var b in U)
                    if (__webpack_require__.o(U, b)) {
                        var d, I = U[b], x = !1, a = !1, o = !1, L = "";
                        switch ((d = I ? s(b) : {
                            type: "disposed",
                            moduleId: b
                        }).chain && (L = "\nUpdate propagation: " + d.chain.join(" -> ")),
                        d.type) {
                            case "self-declined":
                                e.onDeclined && e.onDeclined(d),
                                    e.ignoreDeclined || (x = new Error("Aborted because of self decline: " + d.moduleId + L));
                                break;
                            case "declined":
                                e.onDeclined && e.onDeclined(d),
                                    e.ignoreDeclined || (x = new Error("Aborted because of declined dependency: " + d.moduleId + " in " + d.parentId + L));
                                break;
                            case "unaccepted":
                                e.onUnaccepted && e.onUnaccepted(d),
                                    e.ignoreUnaccepted || (x = new Error("Aborted because " + b + " is not accepted" + L));
                                break;
                            case "accepted":
                                e.onAccepted && e.onAccepted(d),
                                    a = !0;
                                break;
                            case "disposed":
                                e.onDisposed && e.onDisposed(d),
                                    o = !0;
                                break;
                            default:
                                throw new Error("Unexception type " + d.type)
                        }
                        if (x)
                            return {
                                error: x
                            };
                        if (a)
                            for (b in i[b] = I,
                                t(c, d.outdatedModules),
                                d.outdatedDependencies)
                                __webpack_require__.o(d.outdatedDependencies, b) && (l[b] || (l[b] = []),
                                    t(l[b], d.outdatedDependencies[b]));
                        o && (t(c, [d.moduleId]),
                            i[b] = g)
                    }
                U = void 0;
                for (var r, u = [], C = 0; C < c.length; C++) {
                    var G = c[C]
                        , y = __webpack_require__.c[G];
                    y && (y.hot._selfAccepted || y.hot._main) && i[G] !== g && !y.hot._selfInvalidated && u.push({
                        module: G,
                        require: y.hot._requireSelf,
                        errorHandler: y.hot._selfAccepted
                    })
                }
                return {
                    dispose: function () {
                        var Q;
                        F.forEach((function (Q) {
                            delete n[Q]
                        }
                        )),
                            F = void 0;
                        for (var U, B = c.slice(); B.length > 0;) {
                            var e = B.pop()
                                , s = __webpack_require__.c[e];
                            if (s) {
                                var t = {}
                                    , i = s.hot._disposeHandlers;
                                for (C = 0; C < i.length; C++)
                                    i[C].call(null, t);
                                for (__webpack_require__.hmrD[e] = t,
                                    s.hot.active = !1,
                                    delete __webpack_require__.c[e],
                                    delete l[e],
                                    C = 0; C < s.children.length; C++) {
                                    var g = __webpack_require__.c[s.children[C]];
                                    g && (Q = g.parents.indexOf(e)) >= 0 && g.parents.splice(Q, 1)
                                }
                            }
                        }
                        for (var b in l)
                            if (__webpack_require__.o(l, b) && (s = __webpack_require__.c[b]))
                                for (r = l[b],
                                    C = 0; C < r.length; C++)
                                    U = r[C],
                                        (Q = s.children.indexOf(U)) >= 0 && s.children.splice(Q, 1)
                    },
                    apply: function (Q) {
                        for (var U in i)
                            __webpack_require__.o(i, U) && (__webpack_require__.m[U] = i[U]);
                        for (var F = 0; F < B.length; F++)
                            B[F](__webpack_require__);
                        for (var n in l)
                            if (__webpack_require__.o(l, n)) {
                                var s = __webpack_require__.c[n];
                                if (s) {
                                    r = l[n];
                                    for (var t = [], g = [], b = [], d = 0; d < r.length; d++) {
                                        var I = r[d]
                                            , x = s.hot._acceptedDependencies[I]
                                            , a = s.hot._acceptedErrorHandlers[I];
                                        if (x) {
                                            if (-1 !== t.indexOf(x))
                                                continue;
                                            t.push(x),
                                                g.push(a),
                                                b.push(I)
                                        }
                                    }
                                    for (var o = 0; o < t.length; o++)
                                        try {
                                            t[o].call(null, r)
                                        } catch (U) {
                                            if ("function" == typeof g[o])
                                                try {
                                                    g[o](U, {
                                                        moduleId: n,
                                                        dependencyId: b[o]
                                                    })
                                                } catch (F) {
                                                    e.onErrored && e.onErrored({
                                                        type: "accept-error-handler-errored",
                                                        moduleId: n,
                                                        dependencyId: b[o],
                                                        error: F,
                                                        originalError: U
                                                    }),
                                                        e.ignoreErrored || (Q(F),
                                                            Q(U))
                                                }
                                            else
                                                e.onErrored && e.onErrored({
                                                    type: "accept-errored",
                                                    moduleId: n,
                                                    dependencyId: b[o],
                                                    error: U
                                                }),
                                                    e.ignoreErrored || Q(U)
                                        }
                                }
                            }
                        for (var L = 0; L < u.length; L++) {
                            var C = u[L]
                                , G = C.module;
                            try {
                                C.require(G)
                            } catch (U) {
                                if ("function" == typeof C.errorHandler)
                                    try {
                                        C.errorHandler(U, {
                                            moduleId: G,
                                            module: __webpack_require__.c[G]
                                        })
                                    } catch (F) {
                                        e.onErrored && e.onErrored({
                                            type: "self-accept-error-handler-errored",
                                            moduleId: G,
                                            error: F,
                                            originalError: U
                                        }),
                                            e.ignoreErrored || (Q(F),
                                                Q(U))
                                    }
                                else
                                    e.onErrored && e.onErrored({
                                        type: "self-accept-errored",
                                        moduleId: G,
                                        error: U
                                    }),
                                        e.ignoreErrored || Q(U)
                            }
                        }
                        return c
                    }
                }
            }
            self.webpackHotUpdate_panzoom_panzoom = (Q, F, n) => {
                for (var s in F)
                    __webpack_require__.o(F, s) && (U[s] = F[s]);
                n && B.push(n),
                    e[Q] && (e[Q](),
                        e[Q] = void 0)
            }
                ,
                __webpack_require__.hmrI.jsonp = function (Q, n) {
                    U || (U = {},
                        B = [],
                        F = [],
                        n.push(t)),
                        __webpack_require__.o(U, Q) || (U[Q] = __webpack_require__.m[Q])
                }
                ,
                __webpack_require__.hmrC.jsonp = function (e, l, c, i, g, b) {
                    g.push(t),
                        Q = {},
                        F = l,
                        U = c.reduce((function (Q, U) {
                            return Q[U] = !1,
                                Q
                        }
                        ), {}),
                        B = [],
                        e.forEach((function (U) {
                            __webpack_require__.o(n, U) && void 0 !== n[U] && (i.push(s(U)),
                                Q[U] = !0)
                        }
                        )),
                        __webpack_require__.f && (__webpack_require__.f.jsonpHmr = function (U, F) {
                            Q && !__webpack_require__.o(Q, U) && __webpack_require__.o(n, U) && void 0 !== n[U] && (F.push(s(U)),
                                Q[U] = !0)
                        }
                        )
                }
                ,
                __webpack_require__.hmrM = () => {
                    if ("undefined" == typeof fetch)
                        throw new Error("No browser support: need fetch API");
                    return fetch(__webpack_require__.p + __webpack_require__.hmrF()).then((Q => {
                        if (404 !== Q.status) {
                            if (!Q.ok)
                                throw new Error("Failed to fetch update manifest " + Q.statusText);
                            return Q.json()
                        }
                    }
                    ))
                }
        }
        )();
    var __webpack_exports__ = __webpack_require__(634)
}
)();
